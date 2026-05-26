using UnityEngine;

[CreateAssetMenu(fileName = "NewCoreNodeDamagePhase", menuName = "Boss/Core Node Damage Phase Config")]
public class CoreNodeDamagePhaseConfig : PhaseConfig
{
    [Header("Core Node")]
    public CoreNodeIdentifierSO coreNodeIdentifier; 

    [Header("Damage Threshold")]
    public float damageFraction = 0.33f;

    [Header("Meteor Spawners")]
    public MeteorSpawnerGroupSO meteorSpawnerGroup;   // only spawners with this group will activate
}