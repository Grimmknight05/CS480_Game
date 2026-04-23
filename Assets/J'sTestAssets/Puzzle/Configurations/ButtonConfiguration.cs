using UnityEngine;

[CreateAssetMenu(fileName = "ButtonConfig", menuName = "Puzzle/Button Configuration")]
public class ButtonConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class ButtonRequirement : IActivatorRequirement
    {
        public string buttonID;
        public int requiredPresses = 1; // Number of times button must be pressed

        public string ActivatorID => buttonID;

        public bool IsSatisfied(object activatorState)
        {
            if (activatorState is int pressCount)
            {
                return pressCount >= requiredPresses;
            }
            return false;
        }
    }

    [SerializeField] public ButtonRequirement[] requiredButtons;

    public override IActivatorRequirement[] GetRequirements()
    {
        return requiredButtons;
    }
}
