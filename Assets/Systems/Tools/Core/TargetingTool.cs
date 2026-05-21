using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Subclass Sandbox base for tools that aim at a single target along the player's forward vector.
/// Owns the shared targeting/raycast machinery; subclasses supply the hit predicate and on-hit behavior.
/// </summary>
public abstract class TargetingTool : Tool
{
    [Header("Targeting (Cylinder Cast)")]
    [SerializeField, FormerlySerializedAs("lockOnRange")] protected float range = 50f;
    [Tooltip("Radius of the SphereCast sweep along the aim vector. Wider = stronger snap.")]
    [SerializeField] protected float cylinderRadius = 0.5f;

    [Header("Aim Assist (Subtle)")]
    [Tooltip("How close your crosshair needs to be to snap to the target (degrees).")]
    [Range(0f, 45f)]
    [SerializeField, FormerlySerializedAs("lockOnConeAngle")] protected float assistAngle = 10f;
    [Tooltip("How strongly the aim bends toward the target (0 = no assist, 1 = perfect lock).")]
    [Range(0f, 1f)]
    [SerializeField, FormerlySerializedAs("aimAssistStrength")] protected float assistStrength = 0.5f;

    /// <summary>
    /// Subclass predicate: what counts as a valid target for this tool
    /// (e.g., IDamageable for the laser, IKnockbackable for the push).
    /// Called both during cone-snap candidate selection and during the SphereCast hit check.
    /// </summary>
    protected abstract bool IsValidTarget(Collider candidate);

    /// <summary>
    /// Resolves the final aim direction: gun-forward XZ + camera-pitch Y,
    /// then applies a cone-snap blend toward the closest valid target inside assistAngle.
    /// </summary>
    protected Vector3 ResolveAimDirection(Transform firePoint, LayerMask mask, out Transform aimSource)
    {
        IAimContext ctx = firePoint.GetComponentInParent<IAimContext>();
        aimSource = ctx != null ? ctx.AimSource : firePoint;

        // XZ from gun forward, Y (pitch) from camera — preserves the original LaserWeapon feel.
        Vector3 baseDirection = new Vector3(firePoint.forward.x, aimSource.forward.y, firePoint.forward.z).normalized;

        Transform best = FindBestTargetInCone(firePoint, baseDirection, mask);
        if (best == null) return baseDirection;

        Vector3 perfectShot = (best.position - firePoint.position).normalized;
        return Vector3.Lerp(baseDirection, perfectShot, assistStrength).normalized;
    }

    /// <summary>
    /// Cylinder sweep along aimDir. First valid hit wins.
    /// Falls back to a thin Raycast in case the sphere skimmed past a thin collider.
    /// </summary>
    protected bool AcquireTarget(Transform firePoint, Vector3 aimDir, LayerMask mask, out RaycastHit hit)
    {
        if (Physics.SphereCast(firePoint.position, cylinderRadius, aimDir, out hit, range, mask)
            && IsValidTarget(hit.collider))
            return true;

        if (Physics.Raycast(firePoint.position, aimDir, out hit, range, mask)
            && IsValidTarget(hit.collider))
            return true;

        hit = default;
        return false;
    }

    private Transform FindBestTargetInCone(Transform firePoint, Vector3 aimDirection, LayerMask mask)
    {
        Collider[] potentialTargets = Physics.OverlapSphere(firePoint.position, range, mask);
        Transform bestTarget = null;
        float smallestAngle = assistAngle;

        foreach (Collider col in potentialTargets)
        {
            if (!IsValidTarget(col)) continue;

            Vector3 toTarget = (col.transform.position - firePoint.position).normalized;
            float angle = Vector3.Angle(aimDirection, toTarget);

            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestTarget = col.transform;
            }
        }

        return bestTarget;
    }
}
