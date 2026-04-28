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
                else if (Vector3.Distance(transform.position, player.position) <= attackData.attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Attack:
                Attack();
                if (Vector3.Distance(transform.position, player.position) > attackData.attackRange)
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

        if (player == null)
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

        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 2f);
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
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);

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