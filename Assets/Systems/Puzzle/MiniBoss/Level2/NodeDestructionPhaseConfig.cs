using UnityEngine;

[CreateAssetMenu(fileName = "NewNodeDestructionPhaseConfig", menuName = "Boss/Node Destruction Phase Config")]
public class NodeDestructionPhaseConfig : PhaseConfig
{
    [Header("Enemy Waves")]
    public SpawnSO enemySpawnGroup;          // Spawn point group for enemies
    public int wavesToSpawn = 2;
    public int enemiesPerWave = 3;
    [Tooltip("Tag that identifies destroyable boss nodes")]
    public string nodeTag = "Boss node";

    [Tooltip("How many nodes the player must destroy to complete this phase (0 = auto‑detect all nodes with the tag)")]
    public int requiredNodeCount = 0;
        [Tooltip("Prefab to spawn for each node")]
    public GameObject nodePrefab;

    [Tooltip("If nodes are spawned dynamically, provide a SpawnSO group (optional)")]
    public SpawnSO nodeSpawnGroup;
}