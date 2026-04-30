using UnityEngine;

[System.Serializable]
public class StoneRequirementTest
{
    public ActivatorID stoneID;
    public float targetRotation = 0f;
    public float tolerance = 5f;

    public bool IsSatisfied(float currentRotation)
    {
        float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));
        return diff <= tolerance;
    }
}