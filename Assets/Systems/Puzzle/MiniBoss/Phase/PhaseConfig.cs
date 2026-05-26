using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewPhaseConfig", menuName = "Boss/Phase Config")]
public class PhaseConfig : ScriptableObject
{
    public PhaseType phaseType;
    public string phaseName;

    // Optional: you could keep pillarIndex, but we'll use direct reference in PhaseEntry instead
    // public int pillarIndex = -1;
    public PillarSO pillarIdentifier;
    public UnityEvent onPhaseStart;
    public UnityEvent onPhaseComplete;
}