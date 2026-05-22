using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NodeDestructionPhase", menuName = "Boss/Phases/Node Destruction Phase")]
public class NodeDestructionPhaseConfig : PhaseConfig
{
    [Header("Node Configuration")]
    public NodeTarget[] nodes;
    
    [Header("Wave Configuration")]
    public int enemiesPerWave = 5;
    public Transform[] spawnPoints;
    
    [Header("Temporary Weapon")]
    public GameObject tempWeaponPickupPrefab;
    public int weaponUses = 3;
    
    [Header("Visual Effects")]
    public GameObject nodeDestroyEffect;
    
    [Serializable]
    public struct NodeTarget
    {
        public GameObject nodeObject;
        public BossPillar pillar;
        public FloatActivatorChannel healthChannel;
        public ActivatorID id;
    }
}