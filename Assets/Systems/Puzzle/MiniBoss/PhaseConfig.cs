using UnityEngine;
using UnityEngine.Events;

// Enum definitions (can be inside file)
public enum ObjectiveType
{
    SpawnWaves,
    LockDoors,
    KillCount,
    Custom
}

public enum AttackPattern
{
    None,
    PeriodicLasers,
    RandomEnemySpawn,
    ActivateAllPossessed
}

[CreateAssetMenu(fileName = "NewPhaseConfig", menuName = "Boss/Phase Config")]
public class PhaseConfig : ScriptableObject
{
    [Header("Basic")]
    public string phaseName;
    public int phaseIndex;

    [Header("Objective")]
    public ObjectiveType objectiveType;
    public int requiredCompletions = 1;

    // SpawnWaves parameters
    public float waveSpawnInterval = 3f;
    public int enemiesPerWave = 3;
    public Transform[] spawnPoints;         // Drag transforms from scene

    // LockDoors parameters
    public PossessableObject[] doorsToLock;

    // KillCount parameters
    public int totalEnemiesToKill = 15;
    public float enemySpawnInterval = 0.8f;

    [Header("Attacks")]
    public AttackPattern attackPattern;
    public float attackInterval = 5f;

    [Header("Stone Requirement (optional)")]
    // You can reuse your existing StoneRequirement from StoneConfiguration if you make it public and reference the type.
    // For simplicity, add a string and float here:
    public bool requiresStone = false;
    public string targetStoneID;
    public float targetRotation = 0f;

    [Header("Events")]
    public UnityEvent onPhaseStart;
    public UnityEvent onPhaseComplete;
}