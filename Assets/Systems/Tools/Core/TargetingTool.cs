using UnityEngine;
using UnityEngine.Serialization;

public abstract class TargetingTool : Tool
{
    [Header("Targeting (Cylinder Cast)")]
    [SerializeField] protected float range = 50f;
    [SerializeField] protected float cylinderRadius = 0.5f;

    [Header("Aim Assist (Subtle)")]
    [Range(0f, 45f)]
    [SerializeField] protected float assistAngle = 10f;
    [Range(0f, 1f)]
    [SerializeField] protected float assistStrength = 0.5f;

    /// <summary>
    /// Subclass predicate: what counts as a valid target for this tool.
    /// </summary>
    protected abstract bool IsValidTarget(Collider candidate);

    /// <summary>
    /// Override this to customise where the weapon aims on a target (e.g., chest bone).
    /// Default uses the collider's geometric center.
    /// </summary>
    protected virtual Vector3 GetTargetPoint(Collider targetCollider)
    {
        if (targetCollider == null) return Vector3.zero;
        return targetCollider.bounds.center;
    }

    /// <summary>
    /// Resolves the final aim direction: gun-forward XZ + camera‑pitch Y,
    /// then applies a cone‑snap blend toward the closest valid target inside assistAngle.
    /// </summary>
    protected Vector3 ResolveAimDirection(Transform firePoint, LayerMask mask, out Transform aimSource)
    {
        IAimContext ctx = firePoint.GetComponentInParent<IAimContext>();
        aimSource = ctx != null ? ctx.AimSource : firePoint;

        Vector3 baseDirection = new Vector3(firePoint.forward.x, aimSource.forward.y, firePoint.forward.z).normalized;

        Transform best = FindBestTargetInCone(firePoint, baseDirection, mask);
        if (best == null) return baseDirection;

        Collider bestCollider = best.GetComponent<Collider>();
        if (bestCollider == null) return baseDirection;

        Vector3 targetPoint = GetTargetPoint(bestCollider);
        Vector3 perfectShot = (targetPoint - firePoint.position).normalized;
        return Vector3.Lerp(baseDirection, perfectShot, assistStrength).normalized;
    }

    /// <summary>
    /// Cylinder sweep along aimDir. First valid hit wins.
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

            // Use target point for angle calculation
            Vector3 targetPoint = GetTargetPoint(col);
            Vector3 toTarget = (targetPoint - firePoint.position).normalized;
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