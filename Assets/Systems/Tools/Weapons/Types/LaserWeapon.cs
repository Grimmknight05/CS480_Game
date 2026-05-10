using UnityEngine;

[CreateAssetMenu(fileName = "LaserWeapon", menuName = "Weapons/Laser")]
public class LaserWeapon : Weapon
{
    [Header("Laser Settings")]
    [SerializeField] private float range = 50f;
    [SerializeField] private GameObject laserPrefab;

    [Header("Aim Assist (Subtle)")]
    [Tooltip("How close your crosshair needs to be to snap to the target (e.g., 10 degrees).")]
    [SerializeField] private float assistAngle = 10f;
    [Tooltip("How strongly the laser bends toward the target (0 = no assist, 1 = perfect lock).")]
    [Range(0f, 1f)]
    [SerializeField] private float assistStrength = 0.5f;

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {
        PlayUseSound(audioSource);

        // 1. Grab Camera Context
        IAimContext aimContext = firePoint.GetComponentInParent<IAimContext>();
        Transform cameraTransform = aimContext != null ? aimContext.AimSource : firePoint;

        // 2. Base Direction (Player XZ + Camera Y Pitch)
        Vector3 baseDirection = new Vector3(firePoint.forward.x, cameraTransform.forward.y, firePoint.forward.z).normalized;
        Vector3 finalDirection = baseDirection;

        // 3. THE "LITTLE BIT" OF AUTO-AIM
        Transform targetObject = FindBestTargetInTightCone(firePoint, baseDirection, layerMask);

        if (targetObject != null)
        {
            // Calculate the perfect shot to the center of the target
            Vector3 perfectShot = (targetObject.position - firePoint.position).normalized;
            
            // Blend your raw aim with the perfect shot based on the assistStrength
            finalDirection = Vector3.Lerp(baseDirection, perfectShot, assistStrength).normalized;
            Debug.Log($"<color=cyan>[Laser Aim Assist]</color> Magnetized to {targetObject.name}");
        }

        // 4. Fire the Raycast
        Vector3 endPoint = firePoint.position + finalDirection * range;

        if (Physics.Raycast(firePoint.position, finalDirection, out RaycastHit hit, range, layerMask))
        {
            endPoint = hit.point;

            // Damage Enemies
            var damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerHit);
            }

            // Trigger Puzzles & Status Effects
            ApplyEffects(hit.collider.gameObject, finalDirection);
        }

        // 5. Draw the Visuals
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

    private Transform FindBestTargetInTightCone(Transform firePoint, Vector3 aimDirection, int layerMask)
    {
        Collider[] potentialTargets = Physics.OverlapSphere(firePoint.position, range, layerMask);
        Transform bestTarget = null;
        
        // Start with the maximum allowed angle. We want to find the target CLOSEST to the crosshair.
        float smallestAngle = assistAngle; 

        foreach (Collider col in potentialTargets)
        {
            // Must be an enemy or a puzzle object
            if (col.GetComponent<IDamageable>() == null && col.GetComponent<IBurnable>() == null)
                continue;

            Vector3 toTarget = (col.transform.position - firePoint.position).normalized;
            float angle = Vector3.Angle(aimDirection, toTarget);

            // If this target is within our tight cone AND closer to the center of the reticle than the last one we checked
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestTarget = col.transform;
            }
        }

        return bestTarget;
    }
}