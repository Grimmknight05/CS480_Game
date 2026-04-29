using UnityEngine;

[CreateAssetMenu(fileName = "CamShakeConfig", menuName = "Camera/Shake Config")]
public class CamShakeConfig : ScriptableObject
{
    public float magnitude = 0.25f;
    public float duration = 0.4f;

    // Optional future-proofing
    public AnimationCurve intensityOverTime;
    public bool useCurve = false;
    
}