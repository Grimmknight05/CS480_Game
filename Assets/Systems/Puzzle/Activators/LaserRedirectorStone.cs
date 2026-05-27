using UnityEngine;

public class LaserRedirectorStone : MonoBehaviour
{
    [Header("Redirect Direction")]
    [Tooltip("The laser exits in this transform's forward direction. Leave empty to use this GameObject's forward direction.")]
    [SerializeField] private Transform outputDirection;

    [Header("Turn Trigger Targeting")]
    [Tooltip("Optional target used by rotating turn-trigger puzzles. At the aligned Y rotation, the laser exits toward this target. Other faces rotate that direction around Y.")]
    [SerializeField] private GameObject alignedTarget;
    [SerializeField] private float alignedWorldYRotation = 90f;

    [Header("Input Rules")]
    [Tooltip("When enabled, the laser must hit the front side of this stone before it redirects.")]
    [SerializeField] private bool requireFrontHit;
    [SerializeField] private float maxFrontHitAngle = 115f;

    [Header("Optional Reflection")]
    [Tooltip("Use physical reflection from the hit normal instead of the output transform direction.")]
    [SerializeField] private bool reflectFromHitNormal;

    public bool TryGetRedirectDirection(Vector3 incomingDirection, Vector3 hitNormal, Vector3 hitPoint, out Vector3 redirectedDirection)
    {
        redirectedDirection = Vector3.zero;

        if (requireFrontHit)
        {
            float inputAngle = Vector3.Angle(-incomingDirection.normalized, transform.forward);
            if (inputAngle > maxFrontHitAngle)
            {
                return false;
            }
        }

        if (alignedTarget != null)
        {
            Vector3 alignedDirection = (alignedTarget.transform.position - hitPoint).normalized;
            float yawOffset = Mathf.DeltaAngle(alignedWorldYRotation, transform.eulerAngles.y);
            redirectedDirection = Quaternion.AngleAxis(yawOffset, Vector3.up) * alignedDirection;
        }
        else if (reflectFromHitNormal)
        {
            redirectedDirection = Vector3.Reflect(incomingDirection.normalized, hitNormal).normalized;
        }
        else
        {
            Transform output = outputDirection != null ? outputDirection : transform;
            redirectedDirection = output.forward.normalized;
        }

        return redirectedDirection.sqrMagnitude > 0.001f;
    }

    private void OnDrawGizmosSelected()
    {
        Transform output = outputDirection != null ? outputDirection : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(output.position, output.forward * 2f);
    }
}
