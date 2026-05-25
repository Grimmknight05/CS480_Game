using UnityEngine;
using System.Collections;

public class BossEnergyProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private int damage;
    private bool isFlying = true;
    
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float explosionRadius = 2f;
    
    public void Initialize(Vector3 target, float moveSpeed, int projectileDamage)
    {
        targetPosition = target;
        speed = moveSpeed;
        damage = projectileDamage;
        
        // Orient to face target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        if (!isFlying) return;
        
        // Move toward target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            Explode();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isFlying) return;
        
        // Only explode on player, not on boss or enemies
        if (other.CompareTag("Player"))
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        isFlying = false;
        
        // Damage player
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                var health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                    health.TakeDamage(damage);
            }
        }
        
        // Visual effect
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
