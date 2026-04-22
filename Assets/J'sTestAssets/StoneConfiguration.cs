using UnityEngine;

[CreateAssetMenu(fileName = "StoneConfig", menuName = "Puzzle/Stone Configuration")]
public class StoneConfiguration : ScriptableObject
{
    [System.Serializable]
    public class StoneRequirement
    {
        public TurnableStone stone;
        public float targetRotation = 0f;
        
        public bool IsSatisfied()
        {
            return stone != null && stone.IsAtTargetRotation();
        }
    }
    
    [SerializeField] public StoneRequirement[] requiredStones;
}
