using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemySpawner : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private GameObject[] enemies = new GameObject[0];
    [SerializeField] private bool hideEnemiesOnAwake = true;

    [Header("Rise Motion")]
    [SerializeField] private float buriedDepth = 2.5f;
    [SerializeField] private float riseDuration = 0.8f;
    [SerializeField] private float staggerDelay = 0.15f;
    [SerializeField] private AnimationCurve riseEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Safety")]
    [SerializeField] private bool disableCollidersWhileRising = true;
    [SerializeField] private bool enableEnemyAIWhenFinished = true;
    [SerializeField] private float navMeshSampleDistance = 2f;

    private Vector3[] finalPositions;
    private bool hasSpawned;

    private void Awake()
    {
        finalPositions = new Vector3[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null) continue;

            finalPositions[i] = enemy.transform.position;

            if (hideEnemiesOnAwake)
                enemy.SetActive(false);
        }
    }

    public void Spawn()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (hasSpawned) return;

        hasSpawned = true;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                StartCoroutine(RiseEnemy(enemies[i], finalPositions[i]));

            if (staggerDelay > 0f)
                yield return new WaitForSeconds(staggerDelay);
        }
    }

    private IEnumerator RiseEnemy(GameObject enemy, Vector3 finalPosition)
    {
        NavMeshAgent[] agents = enemy.GetComponentsInChildren<NavMeshAgent>(true);
        EnemyControllerTest[] enemyControllers = enemy.GetComponentsInChildren<EnemyControllerTest>(true);
        Collider[] colliders = enemy.GetComponentsInChildren<Collider>(true);
        Rigidbody[] rigidbodies = enemy.GetComponentsInChildren<Rigidbody>(true);
        bool[] rigidbodyKinematicStates = new bool[rigidbodies.Length];

        SetAgentsEnabled(agents, false);
        SetEnemyControllersEnabled(enemyControllers, false);

        if (disableCollidersWhileRising)
            SetCollidersEnabled(colliders, false);

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodyKinematicStates[i] = rigidbodies[i].isKinematic;
            rigidbodies[i].isKinematic = true;
        }

        if (NavMesh.SamplePosition(finalPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            finalPosition = hit.position;

        Vector3 startPosition = finalPosition + Vector3.down * buriedDepth;
        enemy.transform.position = startPosition;
        enemy.SetActive(true);

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, riseDuration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float easedT = riseEase != null ? riseEase.Evaluate(t) : t;
            enemy.transform.position = Vector3.Lerp(startPosition, finalPosition, easedT);
            yield return null;
        }

        enemy.transform.position = finalPosition;

        if (disableCollidersWhileRising)
            SetCollidersEnabled(colliders, true);

        for (int i = 0; i < rigidbodies.Length; i++)
            rigidbodies[i].isKinematic = rigidbodyKinematicStates[i];

        SetAgentsEnabled(agents, true);

        foreach (NavMeshAgent agent in agents)
        {
            if (agent != null && agent.enabled)
                agent.Warp(finalPosition);
        }

        if (enableEnemyAIWhenFinished)
            SetEnemyControllersEnabled(enemyControllers, true);
    }

    private void SetAgentsEnabled(NavMeshAgent[] agents, bool enabled)
    {
        foreach (NavMeshAgent agent in agents)
        {
            if (agent != null)
                agent.enabled = enabled;
        }
    }

    private void SetEnemyControllersEnabled(EnemyControllerTest[] enemyControllers, bool enabled)
    {
        foreach (EnemyControllerTest enemyController in enemyControllers)
        {
            if (enemyController != null)
                enemyController.enabled = enabled;
        }
    }

    private void SetCollidersEnabled(Collider[] colliders, bool enabled)
    {
        foreach (Collider collider in colliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }
    }
}
