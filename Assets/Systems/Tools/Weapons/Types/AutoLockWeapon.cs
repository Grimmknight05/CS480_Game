using UnityEngine;

[CreateAssetMenu(fileName = "AutoLockWeapon", menuName = "Weapons/Auto Lock-On Weapon")]
public class AutoLockWeapon : Weapon
{
    [Header("Laser")]
    [SerializeField] private GameObject laserPrefab;

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {
        PlayUseSound(audioSource);

        Vector3 finalDirection = ResolveAimDirection(firePoint, layerMask, out _);
        Vector3 endPoint = firePoint.position + finalDirection * range;

        if (AcquireTarget(firePoint, finalDirection, layerMask, out RaycastHit hit))
        {
            endPoint = hit.point;

            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerHit);
            }

            ApplyEffects(hit.collider.gameObject, finalDirection);
        }

        if (laserPrefab != null)
        {
            GameObject laserObj = Instantiate(laserPrefab);
            LaserBeam laser = laserObj.GetComponent<LaserBeam>();
            if (laser != null)
            {
                laser.Fire(firePoint.position, endPoint);
            }
        }
    }

    protected override bool IsValidTarget(Collider candidate)
    {
        return candidate.GetComponent<IDamageable>() != null;
    }
}
