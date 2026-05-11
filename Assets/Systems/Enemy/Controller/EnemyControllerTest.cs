using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Dead
}

public class EnemyControllerTest : MonoBehaviour //Take in Interface damage for TakeDamage() definition
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform player; // Player's transform
    [SerializeField] private float detectionRange = 10f; // How far the enemy can detect the player
    [SerializeField] private float fieldOfViewAngle = 90f; // The angle of the enemy's field of view

    [SerializeField] private AttackData attackData;
    private float lastAttackTime;
    private NavMeshAgent navMeshAgent;
    private EnemyState currentState = EnemyState.Patrol;
    //Damage system
    private IDamageable playerDamageable;


    // Patrol
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;

    // Sound
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] enemySFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip deathSFX;
    private float soundTimer = 0f;
    [SerializeField] private float enemySoundMinInterval = 3f; // Plays soundFX every 3-5 seconds
    [SerializeField] private float enemySoundMaxInterval = 5f;

    // Animation
    //private Animator animator;

    [Header("Reset")]
    [SerializeField] private LevelResetChannelSO resetChannel;
    private Vector3 startPos;
    private Quaternion startRot;

    [Header("NavMesh fallback")]
    [Tooltip("If there is no baked NavMesh or this agent never lands on it, AI stops and the tag is set so player contact does not count as Enemy.")]
    [SerializeField] private string harmlessTagWhenNoNavMesh = "Untagged";

    private string originalTag;
    private bool navAiEnabled = true;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void OnEnable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised += ResetEnemy;
    }

    void OnDisable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised -= ResetEnemy;
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalTag = gameObject.tag;

        EvaluateNavMeshSupport(logOnce: true);

        //animator = GetComponent<Animator>();
        playRandomSFX(enemySFX);
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerDamageable = player.GetComponent<IDamageable>();
        }

        if (navAiEnabled && navMeshAgent != null && patrolPoints != null && patrolPoints.Length > 0)
        {
            TrySetDestination(patrolPoints[0].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        if (!navAiEnabled)
            return;

        UpdateState();
        HandleSound();
    }

    private void EvaluateNavMeshSupport(bool logOnce = false)
    {
        navAiEnabled = true;

        if (navMeshAgent == null)
        {
            PacifyNoNavMesh($"[{name}] No NavMeshAgent — pacifying enemy.", logOnce);
            return;
        }

        navMeshAgent.enabled = true;

        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.indices == null || triangulation.indices.Length == 0)
        {
            PacifyNoNavMesh($"[{name}] No NavMesh triangles in scene — pacifying enemy.", logOnce);
            return;
        }

        if (!navMeshAgent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            PacifyNoNavMesh($"[{name}] NavMesh exists but agent is not on mesh — pacifying enemy.", logOnce);
        }
    }

    private void PacifyNoNavMesh(string message, bool log)
    {
        navAiEnabled = false;
        if (log)
            Debug.LogWarning(message);

        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (!string.IsNullOrEmpty(harmlessTagWhenNoNavMesh))
            gameObject.tag = harmlessTagWhenNoNavMesh;
    }

    private bool TrySetDestination(Vector3 destination)
    {
        if (!navAiEnabled || navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            return false;

        navMeshAgent.SetDestination(destination);
        return true;
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (CanSeePlayer())
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
            case EnemyState.Chase:
                Chase();
                if (player == null || attackData == null)
                    break;
                if (!CanSeePlayer())
                {
                    ChangeState(EnemyState.Patrol);
                }
                else if (Vector3.Distance(transform.position, player.position) <= attackData.attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Attack:
                Attack();
                if (player == null || attackData == null)
                    break;
                if (Vector3.Distance(transform.position, player.position) > attackData.attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }

    private void Patrol()
    {
        if (!navAiEnabled || navMeshAgent == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        navMeshAgent.speed = patrolSpeed;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            TrySetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void Chase()
    {
        if (player == null)
            return;
        navMeshAgent.speed = chaseSpeed;
        TrySetDestination(player.position);
    }

    private void Attack()
    {
        if (!navAiEnabled || navMeshAgent == null || !navMeshAgent.enabled)
            return;

        navMeshAgent.isStopped = true;

        if (player == null || attackData == null)
            return;

        if (Time.time - lastAttackTime < attackData.attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        if (playerDamageable != null)
        {
            playerDamageable.TakeDamage(attackData.attackDamage);
        }

        var runner = player.GetComponent<StatusEffectRunner>();

        if (runner != null && attackData != null)
        {
            foreach (var effect in attackData.effects)
            {
                effect.Apply(player.gameObject, transform.forward);
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angle > fieldOfViewAngle / 2) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, distanceToPlayer))
        {
            return hit.collider.transform == player;
        }
        return true;
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        if (navMeshAgent != null && navMeshAgent.enabled)
            navMeshAgent.isStopped = false;

        /*// Animation triggers
        if (animator != null)
        {
            switch (newState)
            {
                case EnemyState.Patrol:
                    animator.SetTrigger("Patrol");
                    break;
                case EnemyState.Chase:
                    animator.SetTrigger("Chase");
                    break;
                case EnemyState.Attack:
                    animator.SetTrigger("Attack");
                    break;
                case EnemyState.Dead:
                    animator.SetTrigger("Die");
                    break;
            }
        }*/
    }

    public bool IsDead { get; private set; }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        ChangeState(EnemyState.Dead);

        if (navMeshAgent != null && navMeshAgent.enabled)
            navMeshAgent.isStopped = true;

        if (deathSFX != null)
            audioSource.PlayOneShot(deathSFX);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Hide visuals after a delay so the death is readable, but keep the
        // GameObject alive so a level reset can revive this enemy.
        Invoke(nameof(HideOnDeath), 2f);
    }

    private void HideOnDeath()
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
    }

    public void ResetEnemy()
    {
        CancelInvoke(nameof(HideOnDeath));

        IsDead = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;

        EnemyHealth eh = GetComponent<EnemyHealth>();
        if (eh != null) eh.RestoreFull();

        transform.position = startPos;
        transform.rotation = startRot;

        gameObject.tag = originalTag;
        EvaluateNavMeshSupport(logOnce: false);

        if (navAiEnabled && navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            navMeshAgent.Warp(startPos);

        if (navAiEnabled && navMeshAgent != null && navMeshAgent.enabled && patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            TrySetDestination(patrolPoints[0].position);
        }

        currentState = EnemyState.Patrol;
        lastAttackTime = 0f;
    }

    private void HandleSound()
    {
        soundTimer -= Time.deltaTime;
        if (soundTimer <= 0f)
        {
            playRandomSFX(enemySFX);
            float enemySoundInterval = Random.Range(enemySoundMinInterval, enemySoundMaxInterval);
            soundTimer = enemySoundInterval;
        }
    }

    private void playRandomSFX(AudioClip[] soundList)
    {
        if (soundList == null || soundList.Length == 0) return;
        int randomIndex = Random.Range(0, soundList.Length);
        audioSource.PlayOneShot(soundList[randomIndex]);
    }

    // Gizmos for visualization
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackData != null ? attackData.attackRange : 0f);

        // Patrol waypoints
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.5f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints.Length > 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }
}