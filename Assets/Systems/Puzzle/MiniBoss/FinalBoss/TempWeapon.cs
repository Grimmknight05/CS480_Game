using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TempWeapon", menuName = "Tools/Temp Weapon")]
public class TempWeapon : Weapon
{
    [Header("Temp Weapon - Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 30f;

    [Header("Temp Weapon - Ammo")]
    public int maxAmmo = 20;
    public bool infiniteAmmo = false;

    [NonSerialized] public int currentAmmo;
    public event Action<int, int> OnAmmoChanged;
    public event Action OnAmmoDepleted;
    public void Initialize()
    {
        currentAmmo = maxAmmo;
    }

    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {
        if (!infiniteAmmo && currentAmmo <= 0) return;

        // Resolve final aim direction (with auto‑lock cone assist)
        Vector3 aimDir = ResolveAimDirection(usePoint, layerMask, out _);

        // Spawn the projectile
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, usePoint.position, Quaternion.identity);
            TempWeaponProjectile projScript = proj.GetComponent<TempWeaponProjectile>();
            if (projScript != null)
            {
                // Pass damage, effects, target layer, and direction
                projScript.InitializeAsDamaging(damagePerHit, effects, targetLayer, aimDir);
            }
            else
            {
                // Fallback: simple rigidbody movement (no damage)
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = aimDir * projectileSpeed;
            }
        }

        // Ammo & cooldown
        if (!infiniteAmmo)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);

            if (currentAmmo <= 0)
            {
                OnAmmoDepleted?.Invoke();
            }
        }

        PlayUseSound(audioSource);
    }

    private void SpawnProjectile(Transform origin, Vector3 targetPoint)
    {
        if (projectilePrefab == null) return;
        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        Vector3 direction = (targetPoint - origin.position).normalized;
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = direction * projectileSpeed;
    }

    protected override bool IsValidTarget(Collider candidate)
    {
        return candidate.GetComponent<IDamageable>() != null;
    }
}