using UnityEngine;

[CreateAssetMenu(fileName = "LeverConfig", menuName = "Puzzle/Lever Configuration")]
public class LeverConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class LeverRequirement : IActivatorRequirement
    {
        public string leverID;
        public bool mustBeEngaged = true; // Lever must be pulled/pushed to a specific state

        public string ActivatorID => leverID;

        public bool IsSatisfied(object activatorState)
        {
            if (activatorState is bool isEngaged)
            {
                return isEngaged == mustBeEngaged;
            }
            return false;
        }
    }

    [SerializeField] public LeverRequirement[] requiredLevers;

    public override IActivatorRequirement[] GetRequirements()
    {
        return requiredLevers;
    }
}
