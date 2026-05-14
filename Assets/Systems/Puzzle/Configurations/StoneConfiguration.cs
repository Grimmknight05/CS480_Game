using UnityEngine;

[CreateAssetMenu(fileName = "StoneConfig", menuName = "Puzzle/Stone Configuration")]
public class StoneConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class StoneRequirement : IActivatorRequirement<float>
    {
        [SerializeField] private ActivatorID stoneID;
        [Header("Optional Rotation Check")]
        [SerializeField] private bool requireSpecificRotation = true;
        [SerializeField] private float activationRotation = 0.0f;
        [SerializeField] private float rotationTolerance = 0.0f;

        public ActivatorID ActivatorID => stoneID;

        public bool IsSatisfied(float rotation)
        {
            if (!requireSpecificRotation) return true;
            float diff = Mathf.Abs(Mathf.DeltaAngle(rotation, activationRotation));
            return diff <= rotationTolerance;
        }
    }

    [SerializeField] private StoneRequirement[] requiredStones;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (requiredStones == null || requiredStones.Length == 0) return false;
        foreach (var r in requiredStones)
        {
            if (r.ActivatorID == null) return false;
            if (!state.TryGetFloat(r.ActivatorID, out float value)) return false;
            if (!r.IsSatisfied(value)) return false;
        }
        return true;
    }
}
