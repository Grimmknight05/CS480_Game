using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using System.Collections.Generic;

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
    /// <summary>Per-instance combat numbers cloned from <see cref="attackData"/> so pacify/reset never edits the shared asset.</summary>
    private AttackData attackRuntime;
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

    /// <summary>Transforms that start with Enemy tag — cleared on pacify so player contact damage (trigger + tag) stops.</summary>
    private readonly List<Transform> enemyTaggedParts = new List<Transform>();
    private readonly List<string> enemyTaggedOriginalTags = new List<string>();

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        CacheEnemyContactTaggedParts();
        RebuildAttackRuntime();
    }

    private void CacheEnemyContactTaggedParts()
    {
        enemyTaggedParts.Clear();
        enemyTaggedOriginalTags.Clear();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            GameObject go = t.gameObject;
            if (!go.CompareTag("Enemy")) continue;
            enemyTaggedParts.Add(t);
            enemyTaggedOriginalTags.Add(go.tag);
        }
    }

    /// <summary>Rebuilds runtime attack stats from the assigned ScriptableObject (used on spawn and after level reset).</summary>
    private void RebuildAttackRuntime()
    {
        if (attackRuntime != null)
        {
            Destroy(attackRuntime);
            attackRuntime = null;
        }
        if (attackData != null)
            attackRuntime = Instantiate(attackData);
    }

    /// <summary>
    /// Zeros melee attack stats and strips Enemy tags on this hierarchy so player scripts that hurt on touch
    /// (<c>CompareTag("Enemy")</c>) stop applying damage. Tags are restored by <see cref="ResetEnemy"/> on level reset.
    /// </summary>
    public void SetPacifiedCombat()
    {
        if (attackRuntime == null && attackData != null)
            RebuildAttackRuntime();
        if (attackRuntime != null)
        {
            attackRuntime.attackDamage = 0;
            attackRuntime.effects = System.Array.Empty<StatusEffect>();
        }
        ClearEnemyContactTagsForPacify();
    }

    private void ClearEnemyContactTagsForPacify()
    {
        for (int i = 0; i < enemyTaggedParts.Count; i++)
        {
            Transform t = enemyTaggedParts[i];
            if (t == null) continue;
            t.gameObject.tag = "Untagged";
        }
    }

    private void RestoreEnemyContactTagsAfterReset()
    {
        for (int i = 0; i < enemyTaggedParts.Count; i++)
        {
            Transform t = enemyTaggedParts[i];
            if (t == null) continue;
            string orig = i < enemyTaggedOriginalTags.Count ? enemyTaggedOriginalTags[i] : "Enemy";
            t.gameObject.tag = string.IsNullOrEmpty(orig) ? "Enemy" : orig;
        }
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
        //animator = GetComponent<Animator>();
        playRandomSFX(enemySFX);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Get and cache player's health component
        if (player != null)
        {
            playerDamageable = player.GetComponent<IDamageable>();
        }
        if (patrolPoints.Length > 0)
        {
            navMeshAgent.SetDestination(patrolPoints[0].position);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        UpdateState();
        HandleSound();
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
                if (!CanSeePlayer())
                {
                    ChangeState(EnemyState.Patrol);
                }
                else if (attackRuntime != null && Vector3.Distance(transform.position, player.position) <= attackRuntime.attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Attack:
                Attack();
                if (attackRuntime == null || Vector3.Distance(transform.position, player.position) > attackRuntime.attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }

    private void Patrol()
    {
        navMeshAgent.speed = patrolSpeed;
        if (patrolPoints.Length == 0) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void Chase()
    {
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);
    }

    private void Attack()
    {
        navMeshAgent.isStopped = true;

        if (player == null || attackRuntime == null)
            return;

        if (Time.time - lastAttackTime < attackRuntime.attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (attackSFX != null)
            audioSource.PlayOneShot(attackSFX);

        if (playerDamageable != null && attackRuntime.attackDamage > 0)
        {
            playerDamageable.TakeDamage(attackRuntime.attackDamage);
        }

        var runner = player.GetComponent<StatusEffectRunner>();

        if (runner != null && attackRuntime.effects != null)
        {
            foreach (var effect in attackRuntime.effects)
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

        RebuildAttackRuntime();
        RestoreEnemyContactTagsAfterReset();

        if (navMeshAgent != null)
        {
            navMeshAgent.Warp(startPos);
            navMeshAgent.isStopped = false;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                currentPatrolIndex = 0;
                navMeshAgent.SetDestination(patrolPoints[0].position);
            }
        }
        transform.rotation = startRot;
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
        if (soundList.Length == 0) return;
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
        var ar = Application.isPlaying ? attackRuntime : attackData;
        if (ar != null)
            Gizmos.DrawWireSphere(transform.position, ar.attackRange);

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