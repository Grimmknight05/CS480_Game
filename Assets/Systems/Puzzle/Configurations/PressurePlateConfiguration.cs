using UnityEngine;

[CreateAssetMenu(fileName = "PressurePlateConfig", menuName = "Puzzle/Pressure Plate Configuration")]
public class PressurePlateConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class PressurePlateRequirement : IActivatorRequirement
    {
        public string plateID;
        public bool mustBePressed = true;   // true = requires pressed, false = requires released

        public string ActivatorID => plateID;

        public bool IsSatisfied(object activatorState)
        {
            // Pressure plate state is a bool (true = pressed)
            if (activatorState is bool isPressed)
            {
                return isPressed == mustBePressed;
            }
            return false;
        }
    }

    [SerializeField] private PressurePlateRequirement[] requiredPlates;

    public override IActivatorRequirement[] GetRequirements() => requiredPlates;
}