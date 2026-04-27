using UnityEngine;

[CreateAssetMenu(fileName = "AreaEffectTool", menuName = "Tools/Area Effect Tool")]
public class AreaEffectTool : Tool
{
    [Header("Area Effect Settings")]
    [SerializeField] public int damagePerHit = 50;
    [SerializeField] public float effectRadius = 20f;

    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {

        PlayUseSound(audioSource);

        // Find all enemies in explosion radius
        Collider[] enemiesInRange = Physics.OverlapSphere(usePoint.position, effectRadius, layerMask);
        
        Debug.Log($"[AreaEffectTool] Explosion at {usePoint.position}! Hit {enemiesInRange.Length} targets!");

        foreach (Collider collider in enemiesInRange)
        {
            if (!collider.CompareTag("Enemy"))
                continue;

            Vector3 dir = (collider.transform.position - usePoint.position).normalized;

            // damage (your existing system)
            IDamageable enemy = collider.GetComponent<IDamageable>();
            if (enemy != null)
                enemy.TakeDamage(damagePerHit);

            // status effects (NEW SYSTEM)
            foreach (StatusEffect effect in effects)
            {
                effect.Apply(collider.gameObject, dir);
            }
        }
    }
}
