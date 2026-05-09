using UnityEngine;

// ============================================
// COMMAND PATTERN FOR BOSS ACTIONS
// ============================================

/// <summary>
/// Base command for encapsulating boss actions.
/// Allows for queuing, undo/redo, and easy logging.
/// </summary>
public interface IBossCommand
{
    void Execute();
    void Undo();
    
}

/// <summary>
/// Command to spawn enemies in a specific area.
/// Uses factory pattern via the pool.
/// </summary>
public class SpawnEnemiesCommand : IBossCommand
{
    private Boss boss;
    private Vector3 spawnPosition;
    private int enemyCount;
    //private List<BossEnemy> spawnedEnemies;
    //private ObjectPool<BossEnemy> pool;

    /*public SpawnEnemiesCommand(Boss boss, Vector3 position, int count, ObjectPool<BossEnemy> pool)
    {
        this.boss = boss;
        this.spawnPosition = position;
        this.enemyCount = count;
        this.pool = pool;
        this.spawnedEnemies = new List<BossEnemy>();
    }*/

    public void Execute()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            //Vector3 offset = Random.insideUnitSphere * 2f;
            //BossEnemy enemy = pool.Get();
            //enemy.transform.position = spawnPosition + offset;
            //enemy.gameObject.SetActive(true);
            //spawnedEnemies.Add(enemy);
        }
    }

    public void Undo()
    {
        //foreach (var enemy in spawnedEnemies)
        //{
        //    pool.Return(enemy);
        //}
        //spawnedEnemies.Clear();
    }
}

/// <summary>
/// Command to activate possessed objects (doors, lasers).
/// </summary>
public class ActivatePossessionCommand : IBossCommand
{
    private IPossessable target;

    public ActivatePossessionCommand(IPossessable target)
    {
        this.target = target;
    }

    public void Execute()
    {
        target.OnPossessed();
        target.Activate();
    }

    public void Undo()
    {
        target.OnReleasePossession();
        target.Deactivate();
    }
}
