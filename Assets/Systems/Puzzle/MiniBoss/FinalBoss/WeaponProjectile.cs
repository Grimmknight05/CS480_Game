using UnityEngine;

/// <summary>
/// Simple projectile for the temp weapon.
/// - Moves straight at a given speed.
/// - Optionally deals damage and applies status effects.
/// - Spawns an impact effect on collision.
/// </summary>
public class TempWeaponProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed in units per second.")]
    public float speed = 40f;

    [Tooltip("Maximum lifetime in seconds before auto-destroy.")]
    public float maxLifetime = 5f;

    [Header("Impact")]
    [Tooltip("Optional particle/effect prefab spawned on hit.")]
    public GameObject impactEffect;

    // Runtime data (set by the weapon that spawns this projectile)
    private int damage;
    private StatusEffect[] effects;
    private LayerMask targetLayers;
    private Vector3 direction;
    private bool useDamage = false;      // if true, projectile deals damage & effects

    /// <summary>
    /// Initialises the projectile for damage‑dealing mode.
    /// Call this from the weapon before firing.
    /// </summary>
    public void InitializeAsDamaging(int damageAmount, StatusEffect[] statusEffects, LayerMask layers, Vector3 fireDirection)
    {
        damage = damageAmount;
        effects = statusEffects;
        targetLayers = layers;
        direction = fireDirection.normalized;
        useDamage = true;
    }

    /// <summary>
    /// Initialises the projectile for visual‑only mode (no damage, just flies and impacts).
    /// </summary>
    public void InitializeAsVisual(Vector3 fireDirection)
    {
        direction = fireDirection.normalized;
        useDamage = false;
    }

    private void Start()
    {
        if (direction == Vector3.zero)
        {
            Debug.LogError("Projectile direction not set! Destroying.");
            Destroy(gameObject);
            return;
        }
        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If in visual-only mode, just spawn effect and die
        if (!useDamage)
        {
            SpawnImpact();
            Destroy(gameObject);
            return;
        }

        // Check if the hit object is on the allowed layer mask
        if (((1 << other.gameObject.layer) & targetLayers) == 0)
            return;

        // Attempt to damage the target
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            
            // Apply status effects
            if (effects != null)
            {
                foreach (var effect in effects)
                    effect?.Apply(other.gameObject, direction);
            }
        }

        SpawnImpact();
        Destroy(gameObject);
    }

    private void SpawnImpact()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.LookRotation(direction));
    }
}