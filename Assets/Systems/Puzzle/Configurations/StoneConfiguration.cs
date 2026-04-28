using UnityEngine;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot (refactored for generic activator system, April 2026)
[CreateAssetMenu(fileName = "StoneConfig", menuName = "Puzzle/Stone Configuration")]
public class StoneConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class StoneRequirement : IActivatorRequirement
    {
        public string stoneID;
        public float activationRotation = 0.0f;
        public float rotationTolerance = 0.0f;
        
        public string ActivatorID => stoneID;

        public bool IsSatisfied(object activatorState)
        {
            if (activatorState is float rotation)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(rotation, activationRotation));
                return diff <= rotationTolerance;
            }
            return false;
        }
    }
    
    [SerializeField] public StoneRequirement[] requiredStones;

    public override IActivatorRequirement[] GetRequirements()
    {
        return requiredStones;
    }
}
