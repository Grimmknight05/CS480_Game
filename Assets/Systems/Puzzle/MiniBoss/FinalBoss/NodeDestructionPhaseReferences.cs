using UnityEngine;

public class NodeDestructionPhaseReferences : MonoBehaviour
{
    [Header("Required Scene References")]
    public CentralBoss centralBoss;
    public BossPillar[] nodePillars = new BossPillar[3];
    public FloatingNode[] floatingNodes = new FloatingNode[3];
}