using System.Collections.Generic;
using UnityEngine;

public class LaserActivatorAfterEnemiesDefeated : ResettableBehaviour
{
    [Header("Enemy Gate")]
    [SerializeField] private EnemyDeathChannel enemyDeathChannel;
    [SerializeField] private GameObject[] enemiesToDefeat;

    [Header("Laser")]
    [SerializeField] private WallLaserEmitter laserToActivate;
    [SerializeField] private GameObject[] objectsToEnableWithLaser;
    [SerializeField] private bool deactivateLaserOnStart = true;

    private bool laserActivated;
    private readonly HashSet<GameObject> defeatedEnemies = new HashSet<GameObject>();
    private bool warnedAboutMissingController;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (enemyDeathChannel != null)
        {
            enemyDeathChannel.OnEnemyDied += HandleEnemyDied;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (enemyDeathChannel != null)
        {
            enemyDeathChannel.OnEnemyDied -= HandleEnemyDied;
        }
    }

    private void Start()
    {
        if (deactivateLaserOnStart)
        {
            SetLaserActive(false);
        }

        CheckCompletion();
    }

    private void HandleEnemyDied(GameObject deadEnemy)
    {
        GameObject trackedEnemy = FindTrackedEnemy(deadEnemy);
        if (trackedEnemy != null)
        {
            defeatedEnemies.Add(trackedEnemy);
        }

        CheckCompletion();
    }

    private void Update()
    {
        if (!laserActivated)
        {
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (laserActivated || enemiesToDefeat == null || enemiesToDefeat.Length == 0)
        {
            return;
        }

        foreach (GameObject enemy in enemiesToDefeat)
        {
            if (IsEnemyDefeated(enemy))
            {
                continue;
            }

            return;
        }

        ActivateLaser();
    }

    public void ActivateLaser()
    {
        if (laserActivated)
            return;

        laserActivated = true;
        SetLaserActive(true);
    }

    private bool IsEnemyDefeated(GameObject enemy)
    {
        if (enemy == null)
        {
            return true;
        }

        if (defeatedEnemies.Contains(enemy))
        {
            return true;
        }

        EnemyControllerTest[] controllers = enemy.GetComponentsInChildren<EnemyControllerTest>(true);
        if (controllers.Length == 0)
        {
            if (!warnedAboutMissingController)
            {
                warnedAboutMissingController = true;
                Debug.LogWarning($"[LaserActivatorAfterEnemiesDefeated] '{enemy.name}' has no EnemyControllerTest, so it cannot be counted as defeated.", enemy);
            }

            return false;
        }

        foreach (EnemyControllerTest controller in controllers)
        {
            if (controller != null && !controller.IsDead)
            {
                return false;
            }
        }

        defeatedEnemies.Add(enemy);
        return true;
    }

    private GameObject FindTrackedEnemy(GameObject candidate)
    {
        if (candidate == null || enemiesToDefeat == null)
        {
            return null;
        }

        Transform candidateTransform = candidate.transform;
        foreach (GameObject enemy in enemiesToDefeat)
        {
            if (enemy == null)
            {
                continue;
            }

            Transform enemyTransform = enemy.transform;
            if (candidate == enemy ||
                candidateTransform.IsChildOf(enemyTransform) ||
                enemyTransform.IsChildOf(candidateTransform))
            {
                return enemy;
            }
        }

        return null;
    }

    private void SetLaserActive(bool active)
    {
        if (laserToActivate != null)
        {
            laserToActivate.SetActive(active);
        }

        if (objectsToEnableWithLaser == null)
        {
            return;
        }

        foreach (GameObject objectToEnable in objectsToEnableWithLaser)
        {
            if (objectToEnable != null)
            {
                objectToEnable.SetActive(active);
            }
        }
    }

    protected override void ResetInternal()
    {
        laserActivated = false;
        defeatedEnemies.Clear();
        warnedAboutMissingController = false;
        if (deactivateLaserOnStart)
        {
            SetLaserActive(false);
        }
    }
}
