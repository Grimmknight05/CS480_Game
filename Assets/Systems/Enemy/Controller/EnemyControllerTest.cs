using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using System.Collections.Generic;

public enum EnemyState
{
    Patrol,
    Chase,
    Circle,
    Lunge,
    Retreat,
    Attack,
    Dead
}

public class EnemyControllerTest : MonoBehaviour //Take in Interface damage for TakeDamage() definition
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform player; // Player's transform
    [SerializeField] private float detectionRange = 50f; // How far the enemy can detect the player
    [SerializeField] private float fieldOfViewAngle = 90f; // The angle of the enemy's field of view

    [Header("Detection Behavior")]
    [SerializeField] private bool detectPlayerImmediately = false;
    [SerializeField] private bool lungeFirstOnDetection = false;

    [SerializeField] private AttackData attackData;
    /// <summary>Per-instance combat numbers cloned from <see cref="attackData"/> so pacify/reset never edits the shared asset.</summary>
    private AttackData attackRuntime;
    private float lastAttackTime;
    private NavMeshAgent navMeshAgent;
    private Rigidbody enemyRigidbody;
    private EnemyState currentState = EnemyState.Patrol;
    //Damage system
    private IDamageable playerDamageable;


    // Patrol
    [SerializeField] private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Tactical Combat")]
    [SerializeField] private bool useTacticalCombat = false;
    [SerializeField] private float tacticalEngageRange = 50f;
    [SerializeField] private float circleDistance = 4f;
    [SerializeField] private float circleSpeed = 4.25f;
    [SerializeField] private float circlePointTolerance = 0.6f;
    [SerializeField] private float mimicMinDistance = 2.75f;
    [SerializeField] private float mimicMaxDistance = 5.25f;
    [SerializeField] private float mimicSideOffset = 2.25f;
    [SerializeField] private float mimicDecisionMinInterval = 0.45f;
    [SerializeField] private float mimicDecisionMaxInterval = 1.1f;
    [SerializeField] private float minCircleTimeBeforeLunge = 1.1f;
    [SerializeField] private float maxCircleTimeBeforeLunge = 2.4f;
    [SerializeField] private float lungeStartRange = 6f;
    [SerializeField] private float lungeDistance = 3.25f;
    [SerializeField] private float lungeDuration = 0.38f;
    [SerializeField] private float lungeArcHeight = 1.15f;
    [SerializeField] private float lungeDamageTime = 0.65f;
    [SerializeField] private float lungeHitPadding = 0.85f;
    [SerializeField] private float retreatDistance = 3.5f;
    [SerializeField] private float retreatSpeed = 5.5f;
    [SerializeField] private float retreatDuration = 0.75f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    private int circleDirection = 1;
    private float nextLungeTime;
    private float nextMimicDecisionTime;
    private Vector3 circleDestination;
    private Vector3 lungeStart;
    private Vector3 lungeEnd;
    private float lungeTimer;
    private bool lungeDamageApplied;
    private bool isManuallyLunging;
    private bool wasKinematicBeforeLunge;
    private float retreatEndTime;
    private Vector3 retreatStart;
    private Vector3 retreatEnd;
    private float retreatTimer;
    private bool hasOpeningLunged;

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
    [SerializeField] private EnemyDeathChannel deathChannel;
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
        enemyRigidbody = GetComponent<Rigidbody>();
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
        RefreshPlayerReference();

        if (patrolPoints != null && patrolPoints.Length > 0 && navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(patrolPoints[0].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        RefreshPlayerReference();
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
                    if (useTacticalCombat)
                    {
                        ChangeState(EnemyState.Circle);
                    }
                    else
                    {
                        ChangeState(EnemyState.Chase);
                    }
                }
                break;
            case EnemyState.Chase:
                Chase();
                if (!CanSeePlayer())
                {
                    ChangeState(EnemyState.Patrol);
                }
                else if (useTacticalCombat && IsPlayerWithin(tacticalEngageRange))
                {
                    ChangeState(EnemyState.Circle);
                }
                else if (!useTacticalCombat && attackRuntime != null && IsPlayerWithin(attackRuntime.attackRange))
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Circle:
                if (!CanSeePlayer())
                {
                    ChangeState(EnemyState.Chase);
                }
                else
                {
                    CirclePlayer();
                }
                break;
            case EnemyState.Lunge:
                LungeAtPlayer();
                break;
            case EnemyState.Retreat:
                RetreatFromPlayer();
                if (!CanSeePlayer())
                {
                    ChangeState(EnemyState.Chase);
                }
                else if (Time.time >= retreatEndTime)
                {
                    ChangeState(EnemyState.Circle);
                }
                break;
            case EnemyState.Attack:
                Attack();
                if (attackRuntime == null || !IsPlayerWithin(attackRuntime.attackRange))
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }

    private void Patrol()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh) return;

        navMeshAgent.speed = patrolSpeed;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void Chase()
    {
        if (player == null || navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh) return;

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);
    }

    private void CirclePlayer()
    {
        if (player == null) return;

        FacePlayer();

        if (CanStartLunge())
        {
            ChangeState(EnemyState.Lunge);
            return;
        }

        bool needsNewDestination =
            circleDestination == Vector3.zero ||
            Time.time >= nextMimicDecisionTime ||
            HasReachedAgentDestination(circlePointTolerance) ||
            Vector3.Distance(transform.position, circleDestination) <= circlePointTolerance;

        if (needsNewDestination)
        {
            PickCircleDestination();
        }

        MoveTowardCircleDestination();
    }

    private void PickCircleDestination()
    {
        if (player == null) return;

        Vector3 fromPlayer = transform.position - player.position;
        fromPlayer.y = 0f;
        if (fromPlayer.sqrMagnitude < 0.1f)
        {
            fromPlayer = -player.forward;
        }

        if (Random.value < 0.45f)
        {
            circleDirection *= -1;
        }

        float lungeRange = GetLungeStartRange();
        float minHoldDistance = Mathf.Max(0.75f, mimicMinDistance);
        float maxHoldDistance = Mathf.Max(minHoldDistance, mimicMaxDistance > 0f ? mimicMaxDistance : circleDistance);
        maxHoldDistance = Mathf.Min(maxHoldDistance, Mathf.Max(minHoldDistance, lungeRange * 0.85f));

        Vector3 awayFromPlayer = fromPlayer.normalized;
        Vector3 sideDirection = Vector3.Cross(Vector3.up, awayFromPlayer).normalized * circleDirection;
        float holdDistance = Random.Range(minHoldDistance, maxHoldDistance);
        float sideAmount = Random.Range(0.35f, Mathf.Max(0.35f, mimicSideOffset));
        Vector3 candidate = player.position + awayFromPlayer * holdDistance + sideDirection * sideAmount;

        if (TrySampleNavMesh(candidate, out Vector3 sampledPoint))
        {
            circleDestination = sampledPoint;
        }
        else
        {
            circleDestination = candidate;
            circleDirection *= -1;
        }

        nextMimicDecisionTime = Time.time + Random.Range(mimicDecisionMinInterval, mimicDecisionMaxInterval);

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(circleDestination);
        }
    }

    private void MoveTowardCircleDestination()
    {
        if (circleDestination == Vector3.zero) return;

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = circleSpeed;
            if (!navMeshAgent.hasPath || Vector3.Distance(navMeshAgent.destination, circleDestination) > 0.25f)
            {
                navMeshAgent.SetDestination(circleDestination);
            }
            return;
        }

        Vector3 target = circleDestination;
        target.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, target, circleSpeed * Time.deltaTime);
    }

    private bool CanStartLunge()
    {
        return CanStartLunge(false);
    }

    private bool CanStartLunge(bool ignoreCooldown)
    {
        if (player == null || attackRuntime == null) return false;
        if (!IsPlayerWithin(tacticalEngageRange)) return false;
        if (GetHorizontalDistanceToPlayer() > GetLungeStartRange()) return false;
        if (ignoreCooldown) return true;
        if (Time.time < nextLungeTime) return false;
        return Time.time - lastAttackTime >= attackRuntime.attackCooldown;
    }

    private void BeginCircle()
    {
        RestoreAgentAfterManualMove();
        circleDirection = Random.value < 0.5f ? -1 : 1;
        nextLungeTime = Time.time + Random.Range(minCircleTimeBeforeLunge, maxCircleTimeBeforeLunge);
        nextMimicDecisionTime = 0f;
        circleDestination = Vector3.zero;
        PickCircleDestination();
    }

    private void BeginLunge()
    {
        if (player == null) return;

        lungeStart = transform.position;
        lungeTimer = 0f;
        lungeDamageApplied = false;
        UpdateLungeTarget();
        PrepareManualMovement();
    }

    private void LungeAtPlayer()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        float duration = Mathf.Max(0.05f, lungeDuration);
        lungeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lungeTimer / duration);
        if (t < 0.45f)
        {
            UpdateLungeTarget();
        }

        Vector3 nextPosition = Vector3.Lerp(lungeStart, lungeEnd, t);
        nextPosition.y += Mathf.Sin(t * Mathf.PI) * lungeArcHeight;
        transform.position = nextPosition;
        FacePlayer();

        if (!lungeDamageApplied && t >= lungeDamageTime)
        {
            lungeDamageApplied = TryDealAttackDamage();
        }

        if (t >= 1f)
        {
            if (!lungeDamageApplied)
            {
                TryDealAttackDamage();
            }
            ChangeState(EnemyState.Retreat);
        }
    }

    private void UpdateLungeTarget()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.1f)
        {
            toPlayer = transform.forward;
        }

        Vector3 desiredEnd = player.position - toPlayer.normalized * GetPreferredAttackStopDistance();
        Vector3 clampedEnd = Vector3.MoveTowards(lungeStart, desiredEnd, lungeDistance);
        lungeEnd = TrySampleNavMesh(clampedEnd, out Vector3 sampledEnd) ? sampledEnd : clampedEnd;
    }

    private void BeginRetreat()
    {
        if (player == null) return;

        retreatStart = transform.position;
        retreatTimer = 0f;
        retreatEndTime = Time.time + retreatDuration;

        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0f;
        if (awayFromPlayer.sqrMagnitude < 0.1f)
        {
            awayFromPlayer = -transform.forward;
        }

        Vector3 awayDirection = awayFromPlayer.normalized;
        Vector3 sideDirection = Vector3.Cross(Vector3.up, awayDirection).normalized * (Random.value < 0.5f ? -1f : 1f);
        float retreatLength = Random.Range(retreatDistance * 0.45f, retreatDistance);
        float sideStep = Random.Range(0f, retreatDistance * 0.35f);
        Vector3 desiredEnd = transform.position + awayDirection * retreatLength + sideDirection * sideStep;
        retreatEnd = TrySampleNavMesh(desiredEnd, out Vector3 sampledEnd) ? sampledEnd : desiredEnd;
        PrepareManualMovement();
    }

    private void RetreatFromPlayer()
    {
        if (player == null) return;

        float duration = Mathf.Max(0.05f, retreatDuration);
        retreatTimer += Time.deltaTime;
        float t = Mathf.Clamp01(retreatTimer / duration);
        transform.position = Vector3.Lerp(retreatStart, retreatEnd, t);
        FacePlayer();
    }

    private void Attack()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }

        TryDealAttackDamage();
    }

    private bool TryDealAttackDamage()
    {
        if (player == null || attackRuntime == null)
            return false;

        if (!IsPlayerWithin(attackRuntime.attackRange + lungeHitPadding))
            return false;

        if (Time.time - lastAttackTime < attackRuntime.attackCooldown)
            return false;

        lastAttackTime = Time.time;

        if (attackSFX != null && audioSource != null)
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

        return true;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return false;
        if (detectPlayerImmediately) return true;

        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
        if (angle > fieldOfViewAngle / 2) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, distanceToPlayer))
        {
            return hit.collider.transform == player || hit.collider.transform.IsChildOf(player);
        }
        return true;
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        bool leavingManualMovement =
            (currentState == EnemyState.Lunge && newState != EnemyState.Retreat) ||
            (currentState == EnemyState.Retreat && newState != EnemyState.Lunge);

        if (leavingManualMovement && newState != EnemyState.Dead)
        {
            RestoreAgentAfterManualMove();
        }

        currentState = newState;

        bool shouldReleaseAgent =
            newState != EnemyState.Lunge &&
            newState != EnemyState.Retreat;

        if (shouldReleaseAgent && navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }

        switch (newState)
        {
            case EnemyState.Circle:
                BeginCircle();
                break;
            case EnemyState.Lunge:
                hasOpeningLunged = true;
                BeginLunge();
                break;
            case EnemyState.Retreat:
                BeginRetreat();
                break;
        }

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

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
    }

    private bool IsPlayerWithin(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    private float GetHorizontalDistanceToPlayer()
    {
        if (player == null) return float.PositiveInfinity;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude;
    }

    private float GetLungeStartRange()
    {
        float reachableRange = lungeDistance + GetPreferredAttackStopDistance() + lungeHitPadding;
        if (lungeStartRange <= 0f)
        {
            return reachableRange;
        }

        return Mathf.Min(lungeStartRange, reachableRange);
    }

    private float GetPreferredAttackStopDistance()
    {
        if (attackRuntime == null)
        {
            return 1f;
        }

        return Mathf.Clamp(attackRuntime.attackRange * 0.65f, 0.75f, 1.5f);
    }

    private bool HasReachedAgentDestination(float tolerance)
    {
        if (navMeshAgent == null || !navMeshAgent.enabled) return false;
        if (navMeshAgent.pathPending) return false;
        return navMeshAgent.remainingDistance <= tolerance;
    }

    private bool TrySampleNavMesh(Vector3 point, out Vector3 sampledPoint)
    {
        sampledPoint = point;
        int areaMask = navMeshAgent != null ? navMeshAgent.areaMask : NavMesh.AllAreas;
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleDistance, areaMask))
        {
            sampledPoint = hit.position;
            return true;
        }

        return false;
    }

    private void RefreshPlayerReference()
    {
        if (player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        player = playerObject.transform;
        playerDamageable = player.GetComponent<IDamageable>();
    }

    private void PrepareManualMovement()
    {
        if (enemyRigidbody != null && !isManuallyLunging)
        {
            wasKinematicBeforeLunge = enemyRigidbody.isKinematic;
            enemyRigidbody.isKinematic = true;
        }

        isManuallyLunging = true;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
            }

            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }
    }

    private void RestoreAgentAfterManualMove()
    {
        if (navMeshAgent == null)
        {
            RestoreRigidbodyAfterLunge();
            return;
        }

        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        if (TrySampleNavMesh(transform.position, out Vector3 sampledPosition))
        {
            transform.position = sampledPosition;
            navMeshAgent.Warp(transform.position);
        }

        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        if (navMeshAgent.isOnNavMesh)
            navMeshAgent.isStopped = false;

        RestoreRigidbodyAfterLunge();
    }

    private void RestoreRigidbodyAfterLunge()
    {
        if (isManuallyLunging && enemyRigidbody != null)
        {
            enemyRigidbody.isKinematic = wasKinematicBeforeLunge;
        }
        isManuallyLunging = false;
    }

    public bool IsDead { get; private set; }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        deathChannel?.RaiseEvent(gameObject);
        Debug.Log("Raised Death on" + gameObject);
        ChangeState(EnemyState.Dead);

        if (deathSFX != null && audioSource != null)
            audioSource.PlayOneShot(deathSFX);

        RestoreRigidbodyAfterLunge();

        if (navMeshAgent != null)
        {
            if (!navMeshAgent.enabled)
            {
                navMeshAgent.enabled = true;
            }
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
            }
            navMeshAgent.enabled = false;
        }

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
            navMeshAgent.enabled = true;
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
            if (TrySampleNavMesh(startPos, out Vector3 sampledStart))
                navMeshAgent.Warp(sampledStart);

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = false;
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    currentPatrolIndex = 0;
                    navMeshAgent.SetDestination(patrolPoints[0].position);
                }
            }
        }
        transform.rotation = startRot;
        currentState = EnemyState.Patrol;
        lastAttackTime = 0f;
        lungeDamageApplied = false;
        hasOpeningLunged = false;
        RestoreRigidbodyAfterLunge();
        circleDestination = Vector3.zero;
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
        if (soundList == null || soundList.Length == 0 || audioSource == null) return;
        int randomIndex = Random.Range(0, soundList.Length);
        audioSource.PlayOneShot(soundList[randomIndex]);
    }

    // Gizmos for visualization
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (useTacticalCombat)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, tacticalEngageRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, circleDistance);
        }

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
