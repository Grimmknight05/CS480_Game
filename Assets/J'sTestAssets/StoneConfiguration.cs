using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StoneConfig", menuName = "Puzzle/Stone Configuration")]
public class StoneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class StoneRequirement
    {
        public string stoneID;
        public float targetRotation = 0.0f;
        
        public bool IsSatisfied(Dictionary<string, TurnableStone> lookup)
        {
            if (!lookup.TryGetValue(stoneID, out var stone))
                return false;

            float diff = Mathf.Abs(Mathf.DeltaAngle(stone.CurrentRotation, targetRotation));
            return diff <= stone.RotationTolerance;
        }
    }
    
    [SerializeField] public StoneRequirement[] requiredStones;
}
