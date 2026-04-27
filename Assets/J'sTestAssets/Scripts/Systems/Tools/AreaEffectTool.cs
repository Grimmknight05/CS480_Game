using UnityEngine;

[CreateAssetMenu(fileName = "AreaEffectTool", menuName = "Tools/Area Effect Tool")]
public class AreaEffectTool : Tool
{
    [Header("Area Effect Settings")]
    [SerializeField] public int damagePerHit = 50;
    [SerializeField] public float effectRadius = 20f;
    [SerializeField] public float knockbackForce = 10f;

    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {

        PlayUseSound(audioSource);

        // Find all enemies in explosion radius
        Collider[] enemiesInRange = Physics.OverlapSphere(usePoint.position, effectRadius, layerMask);
        
        Debug.Log($"[AreaEffectTool] Explosion at {usePoint.position}! Hit {enemiesInRange.Length} targets!");

        foreach (Collider collider in enemiesInRange)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyControllerTest enemy = collider.GetComponent<EnemyControllerTest>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerHit);
                    
                    // Optional: apply knockback
                    Rigidbody rb = collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 knockbackDir = (collider.transform.position - usePoint.position).normalized;
                        rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
