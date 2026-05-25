using UnityEngine;

[CreateAssetMenu(fileName = "BossFinalPhase", menuName = "Boss/Phases/Final Phase")]
public class BossFinalPhaseConfig : PhaseConfig
{
    [Header("Final Phase Settings")]
    [Tooltip("Duration before boss auto-heals if not damaged enough")]
    public float phaseTimeout = 120f;
    
    [Tooltip("Optional: Victory effect when boss dies")]
    public GameObject victoryEffectPrefab;
}
