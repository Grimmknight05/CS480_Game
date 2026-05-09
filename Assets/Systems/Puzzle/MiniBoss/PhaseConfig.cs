using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "NewPhaseConfig", menuName = "Boss/Phase Config")]
public class PhaseConfig : ScriptableObject
{
    [Header("Common")]
    public PhaseType phaseType;
    public string phaseName;

    // For WaveSpawn phase
    public Transform[] spawnPoints;          // assign from scene (drag transforms)
    public int wavesToSpawn = 2;
    public int enemiesPerWave = 3;
    public float spawnInterval = 2f;

    // For DoorLock phase
    //public PossessableObject[] doorsToLock;

    // For both (optional)
    public UnityEvent onPhaseStart;
    public UnityEvent onPhaseComplete;
}