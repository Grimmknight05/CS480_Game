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

        Transform targetEnemy = FindNearestEnemy(firePoint, layerMask);
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
        Vector3 endPoint;

        if (Physics.Raycast(firePoint.position, finalDirection, out hit, lockOnRange, layerMask))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyControllerTest enemy = hit.collider.GetComponent<EnemyControllerTest>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerHit);
                }
            }
        }
        else
        {
            endPoint = firePoint.position + finalDirection * lockOnRange;
        }

        // 🔥 Spawn laser
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
    private Transform FindNearestEnemy(Transform firePoint, int layerMask)
    {
        Collider[] enemies = Physics.OverlapSphere(firePoint.position, lockOnRange, layerMask);

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in enemies)
        {
            if (!col.CompareTag("Enemy"))
                continue;

            Vector3 toEnemy = (col.transform.position - firePoint.position).normalized;
            float angle = Vector3.Angle(firePoint.forward, toEnemy);

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