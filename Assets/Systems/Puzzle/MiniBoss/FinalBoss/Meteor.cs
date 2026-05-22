using UnityEngine;

public class Meteor : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float damage;
    private bool isFlying = true;
    
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 3f;
    
    public void Initialize(Vector3 target, float moveSpeed, float meteorDamage)
    {
        targetPosition = target;
        speed = moveSpeed;
        damage = meteorDamage;
        
        // Look at target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    void Update()
    {
        if (!isFlying) return;
        
        transform.position += transform.forward * speed * Time.deltaTime;
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        isFlying = false;
        
        // Damage in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                var health = hit.GetComponent<PlayerHealth>();
                health?.TakeDamage(Mathf.RoundToInt(damage));
            }
        }
        
        // Visual effect
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}