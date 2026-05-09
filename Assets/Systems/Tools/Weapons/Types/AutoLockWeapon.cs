using UnityEngine;

[CreateAssetMenu(fileName = "AutoLockWeapon", menuName = "Weapons/Auto Lock-On Weapon")]
public class AutoLockWeapon : Weapon
{
    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRange = 50f;
    [SerializeField] private float lockOnConeAngle = 45f;

    [Header("Aim Assist")]
    [Range(0f, 1f)]
    [SerializeField] private float aimAssistStrength = 0.4f;

    [Header("Laser")]
    [SerializeField] private GameObject laserPrefab;

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {
        PlayUseSound(audioSource);

        Vector3 forward = firePoint.forward;
        Vector3 finalDirection = forward;

        Transform targetEnemy = FindNearestValidTarget(firePoint, layerMask);

        Debug.Log($"Has Target: {targetEnemy != null}");

        Debug.DrawRay(firePoint.position, forward * 10f, Color.blue, 1f);
        Debug.DrawRay(firePoint.position, finalDirection * 10f, Color.red, 1f);

        if (targetEnemy != null)
        {
            Vector3 toTarget = (targetEnemy.position - firePoint.position).normalized;
            float angle = Vector3.Angle(forward, toTarget);

            Debug.DrawLine(firePoint.position, targetEnemy.position, Color.green, 1f);

            if (angle <= lockOnConeAngle)
            {
                Debug.Log($"Assist Applied | Angle: {angle}");
                finalDirection = Vector3.Lerp(forward, toTarget, aimAssistStrength).normalized;
            }
        }

        RaycastHit hit;
        Vector3 endPoint = firePoint.position + finalDirection * lockOnRange;

        if (Physics.Raycast(firePoint.position, finalDirection, out hit, lockOnRange, layerMask))
        {
            endPoint = hit.point;

            // 1. Damage (Only applies if it has health)
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerHit);
            }

            // 2. Status Effects (Safely applies to enemies OR puzzles via the base helper)
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

    private Transform FindNearestValidTarget(Transform firePoint, int layerMask)
    {
        Collider[] potentialTargets = Physics.OverlapSphere(firePoint.position, lockOnRange, layerMask);

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in potentialTargets)
        {
            // Surgical Fix: Lock onto anything that can take damage or be burned, ignoring tags.
            bool isValidTarget = col.GetComponent<IDamageable>() != null || col.GetComponent<IBurnable>() != null;
            if (!isValidTarget)
                continue;

            Vector3 toTarget = (col.transform.position - firePoint.position).normalized;
            float angle = Vector3.Angle(firePoint.forward, toTarget);

            if (angle > lockOnConeAngle)
                continue;

            float dist = Vector3.Distance(firePoint.position, col.transform.position);

            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearest = col.transform;
            }
        }

        return nearest;
    }
}