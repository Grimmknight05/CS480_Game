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

        // Find all colliders in explosion radius within the target layer(s)
        Collider[] hits = Physics.OverlapSphere(usePoint.position, effectRadius, layerMask);
        
        Debug.Log($"[AreaEffectTool] Explosion at {usePoint.position}! Hit {hits.Length} targets!");

        foreach (Collider hitCollider in hits)
        {
            // Calculate direction from the blast center to the target
            Vector3 dir = (hitCollider.transform.position - usePoint.position).normalized;

            // 1. Apply Damage (Only works if the object has health, like an enemy)
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerHit);
            }

            // 2. Apply Status Effects using the shared helper in the base Tool class
            // This safely hits BOTH enemies and puzzle objects (the BurnEffect will figure out how to handle it).
            ApplyEffects(hitCollider.gameObject, dir);
        }
    }
}