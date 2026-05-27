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
        public bool RequireSpecificRotation => requireSpecificRotation;
        public float ActivationRotation => activationRotation;
        public float RotationTolerance => rotationTolerance;

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

    public string GetDebugInfo(IPuzzleStateProvider state)
    {
        if (requiredStones == null || requiredStones.Length == 0)
        {
            return $"{name}: no required stones configured.";
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder($"{name}:");
        foreach (var r in requiredStones)
        {
            if (r == null)
            {
                builder.Append(" [missing requirement]");
                continue;
            }

            string stoneName = r.ActivatorID != null ? r.ActivatorID.name : "missing ActivatorID";
            if (r.ActivatorID == null)
            {
                builder.Append($" [{stoneName}]");
                continue;
            }

            if (!state.TryGetFloat(r.ActivatorID, out float value))
            {
                builder.Append($" [{stoneName}: no state received yet]");
                continue;
            }

            float diff = r.RequireSpecificRotation ? Mathf.Abs(Mathf.DeltaAngle(value, r.ActivationRotation)) : 0f;
            bool satisfied = r.IsSatisfied(value);
            builder.Append($" [{stoneName}: current={value:0.##}, target={r.ActivationRotation:0.##}, diff={diff:0.##}, tolerance={r.RotationTolerance:0.##}, solved={satisfied}]");
        }

        return builder.ToString();
    }
}
