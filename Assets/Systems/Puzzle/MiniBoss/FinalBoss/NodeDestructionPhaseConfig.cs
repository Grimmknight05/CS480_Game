using UnityEngine;

[CreateAssetMenu(fileName = "NodeDestructionPhase", menuName = "Boss/Phases/Node Destruction Phase")]
public class NodeDestructionPhaseConfig : PhaseConfig
{
    [Header("Scene References (Assign in Inspector with scene open)")]
    public CentralBoss centralBoss;
    public BossPillar[] pillars = new BossPillar[3];
    public FloatingNode[] nodes = new FloatingNode[3];
    
    [Header("Puzzle Stones")]
    public StoneRequirement[] requiredStones;
    public FloatActivatorChannel stoneStateChannel;
    
    [Header("Enemy Waves")]
    public SpawnSO spawnPointGroup;
    public int enemiesPerWave = 5;
    public float enemyRespawnDelay = 3f;
    
    [Header("Boss Damage")]
    public float bossHealthPerPhase = 100f;
    
    [Header("Temporary Weapon")]
    public TempNodeWeapon tempNodeWeapon;
    
    [Header("Effects")]
    public GameObject nodeDestroyEffect;
    
    [System.Serializable]
    public class StoneRequirement
    {
        public ActivatorID stoneID;
        [SerializeField] private float activationRotation = 0f;
        [SerializeField] private float rotationTolerance = 1f;
        
        public bool IsSatisfied(float currentRotation)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, activationRotation));
            return diff <= rotationTolerance;
        }
    }
}