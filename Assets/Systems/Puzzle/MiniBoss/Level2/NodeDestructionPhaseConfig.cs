using UnityEngine;

[CreateAssetMenu(fileName = "NewNodeDestructionPhaseConfig", menuName = "Boss/Node Destruction Phase Config")]
public class NodeDestructionPhaseConfig : PhaseConfig
{
    [Tooltip("Tag that identifies destroyable boss nodes")]
    public string nodeTag = "Boss node";

    [Tooltip("How many nodes the player must destroy to complete this phase (0 = auto‑detect all nodes with the tag)")]
    public int requiredNodeCount = 0;
        [Tooltip("Prefab to spawn for each node")]
    public GameObject nodePrefab;

    [Tooltip("If nodes are spawned dynamically, provide a SpawnSO group (optional)")]
    public SpawnSO nodeSpawnGroup;
}