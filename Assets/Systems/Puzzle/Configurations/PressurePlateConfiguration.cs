using UnityEngine;

[CreateAssetMenu(fileName = "PressurePlateConfig", menuName = "Puzzle/Pressure Plate Configuration")]
public class PressurePlateConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class PressurePlateRequirement : IActivatorRequirement<bool>
    {
        [SerializeField] private ActivatorID plateID;
        [SerializeField] private bool mustBePressed = true;

        public ActivatorID ActivatorID => plateID;
        public bool IsSatisfied(bool state) => state == mustBePressed;
    }

    [SerializeField] private PressurePlateRequirement[] requiredPlates;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (requiredPlates == null || requiredPlates.Length == 0) return false;
        foreach (var r in requiredPlates)
        {
            if (r.ActivatorID == null) return false;
            if (!state.TryGetBool(r.ActivatorID, out bool value)) return false;
            if (!r.IsSatisfied(value)) return false;
        }
        return true;
    }
}
