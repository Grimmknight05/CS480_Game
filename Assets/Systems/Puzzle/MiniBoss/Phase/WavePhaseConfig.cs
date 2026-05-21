using UnityEngine;

[CreateAssetMenu(fileName = "NewWavePhaseConfig", menuName = "Boss/Wave Phase Config")]
public class WavePhaseConfig : PhaseConfig
{
    [Tooltip("Name of the spawn point group (SpawnPoint component) to use for this phase")]
    public SpawnSO spawnPointGroup;

    public int wavesToSpawn = 2;
    public int enemiesPerWave = 3;
    public float spawnInterval = 2f;
}