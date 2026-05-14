using System;
using UnityEngine;


[CreateAssetMenu(fileName = "NewLeverConfig", menuName = "Puzzle/Lever Configuration")]
public class LeverConfiguration : ActivatorConfiguration
{
    [Serializable]
    public class LeverRequirement : IActivatorRequirement<bool>
    {
        [SerializeField] private ActivatorID leverID;
        [SerializeField] private bool mustBeEngaged = true;

        public ActivatorID ActivatorID => leverID;
        public bool IsSatisfied(bool state) => state == mustBeEngaged;
    }

    [SerializeField] private LeverRequirement[] requiredLevers;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (requiredLevers == null || requiredLevers.Length == 0) return false;
        foreach (var r in requiredLevers)
        {
            if (r.ActivatorID == null) return false;
            if (!state.TryGetBool(r.ActivatorID, out bool value)) return false;
            if (!r.IsSatisfied(value)) return false;
        }
        return true;
    }
}