using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(false, null, null, "AirBlastTool")]
[CreateAssetMenu(fileName = "PushTool", menuName = "Tools/Push Tool")]
public class PushTool : TargetingTool
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 18f;
    [SerializeField] private float stunDuration = 0.25f;
    [SerializeField] private GameObject pushVfxPrefab;

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {
        PlayUseSound(audioSource);

        Vector3 aimDir = ResolveAimDirection(firePoint, layerMask, out _);
        Vector3 endPoint = firePoint.position + aimDir * range;

        if (AcquireTarget(firePoint, aimDir, layerMask, out RaycastHit hit))
        {
            endPoint = hit.point;

            var knockbackable = hit.collider.GetComponent<IKnockbackable>();
            knockbackable.ApplyKnockback(aimDir, pushForce, stunDuration);
            ApplyEffects(hit.collider.gameObject, aimDir);
        }

        if (pushVfxPrefab != null)
        {
            GameObject vfx = Instantiate(pushVfxPrefab, firePoint.position, Quaternion.LookRotation(aimDir));
            LaserBeam beam = vfx.GetComponent<LaserBeam>();
            if (beam != null)
            {
                beam.Fire(firePoint.position, endPoint);
            }
        }
    }

    protected override bool IsValidTarget(Collider candidate)
    {
        return candidate.GetComponent<IKnockbackable>() != null;
    }
}
