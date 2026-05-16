using UnityEngine;

[CreateAssetMenu(fileName = "MushroomConfig", menuName = "Puzzle/Mushroom Configuration")]
public class MushroomConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class MushroomRequirement : IActivatorRequirement<MushroomColor>
    {
        [Tooltip("Must match the ActivatorID SO wired to the MushroomColorChannel for this mushroom.")]
        [SerializeField] private ActivatorID mushroomID;

        [Tooltip("If off, any activation (regardless of color) satisfies this requirement.")]
        [SerializeField] private bool requireSpecificColor = false;

        [SerializeField] private MushroomColor expectedColor = MushroomColor.Green;

        public ActivatorID ActivatorID => mushroomID;

        public bool IsSatisfied(MushroomColor color)
        {
            if (!requireSpecificColor) return true;
            return color == expectedColor;
        }
    }

    [SerializeField] private MushroomRequirement[] required;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (required == null || required.Length == 0) return false;
        foreach (var r in required)
        {
            if (r.ActivatorID == null) return false;
            if (!state.TryGetMushroomColor(r.ActivatorID, out MushroomColor color)) return false;
            if (!r.IsSatisfied(color)) return false;
        }
        return true;
    }
}
