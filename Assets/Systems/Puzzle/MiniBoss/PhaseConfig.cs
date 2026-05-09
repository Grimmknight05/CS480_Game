using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "NewPhaseConfig", menuName = "Boss/Phase Config")]
public class PhaseConfig : ScriptableObject
{
    public PhaseType phaseType;
    public string phaseName;

    // WaveSpawn settings
    public Transform[] spawnPoints;
    public int wavesToSpawn = 2;
    public int enemiesPerWave = 3;
    public float spawnInterval = 2f;

    // DoorLock settings
    public DoorLerp[] doorsToLock;

    // Events (optional)
    public UnityEvent onPhaseStart;
    public UnityEvent onPhaseComplete;
}