using UnityEngine;


[CreateAssetMenu(fileName = "ProjectileWeapon", menuName = "Weapons/Projectile Weapon")]
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public float projectileSpeed = 20f;
    [SerializeField] public int projectileCount = 1; // Multiple projectiles per shot (shotgun-style)

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {

        PlayUseSound(audioSource);

        for (int i = 0; i < projectileCount; i++)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[ProjectileWeapon] No projectilePrefab assigned!");
                return;
            }

            Vector3 spawnPos = firePoint.position;
            Vector3 direction = firePoint.forward;

            if (projectileCount > 1)
            {
                float spreadAngle = 15f;
                direction = Quaternion.Euler(
                    Random.Range(-spreadAngle, spreadAngle),
                    Random.Range(-spreadAngle, spreadAngle),
                    0
                ) * direction;
            }

            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            IProjectile proj = projectile.GetComponent<IProjectile>();
            if (proj != null)
            {
                proj.Initialize(direction * projectileSpeed, damagePerHit, firePoint);
            }
            else
            {
                Debug.LogWarning($"[ProjectileWeapon] Projectile prefab missing IProjectile interface!");
            }
        }
    }
}
