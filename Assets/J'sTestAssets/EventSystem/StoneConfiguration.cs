using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StoneConfig", menuName = "Puzzle/Stone Configuration")]
public class StoneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class StoneRequirement
    {
        public string stoneID;
        public float activationRotation = 0.0f;
        public float rotationTolerance = 0.0f;
        
        public bool IsSatisfied(float currentRotation)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, activationRotation));
            return diff <= rotationTolerance;
        }
    }
    
    [SerializeField] public StoneRequirement[] requiredStones;
}
