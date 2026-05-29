using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Header("Settings")]
    public float fallSpeed = 10f;
    public int damageAmount = 25;
    
    [Header("Status Effects (applied on hit)")]
    [SerializeField] private StatusEffect[] onHitEffects; // drag BurnEffect, KnockbackEffect, etc.
    
    [Header("Audio")]
    [SerializeField] public AudioClip hitSFX;
    [SerializeField] private AudioSource audioSource;
    private ObjectPool<Meteor> myPool;   // reference to the pool that owns this meteor
    private Vector3 targetGroundPoint;
    private bool isFalling;

    public void Initialize(ObjectPool<Meteor> pool, Vector3 startPos)
    {
        myPool = pool;
        transform.position = startPos;
        targetGroundPoint = new Vector3(startPos.x, 0, startPos.z);
        isFalling = true;
    }

    void Update()
    {
        if (!isFalling) return;
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        //Debug.Log($"Position Y: {transform.position.y}");
        if (transform.position.y <= targetGroundPoint.y)
            ReturnToPool();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Trigger")|| other.gameObject.layer == LayerMask.NameToLayer("Liquid")) return;

        //Debug.Log($"OnTriggerEnter with: {other.name}, layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");
        if (!other.CompareTag("Player"))
        {
            if (hitSFX != null && audioSource != null)
                AudioSource.PlayClipAtPoint(hitSFX, transform.position);
            ReturnToPool();
            return;
        }
        // 1. Damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            Debug.Log($"[Meteor] Hit {other.name} for {damageAmount} damage");
        }
        else
        {
            Debug.Log($"[Meteor] {other.name} has no IDamageable");
        }
        if (hitSFX != null && audioSource != null)
            audioSource.PlayOneShot(hitSFX);
            //AudioSource.PlayClipAtPoint(hitSFX, transform.position);
        // 2. Apply all status effects (burn, knockback, etc.)
        if (onHitEffects != null && onHitEffects.Length > 0)
        {
            // Calculate hit direction for knockback (away from meteor impact point)
            Vector3 hitDir = (other.transform.position - transform.position).normalized;

            foreach (StatusEffect effect in onHitEffects)
            {
                if (effect != null)
                    effect.Apply(other.gameObject, hitDir);
            }
        }
        ReturnToPool();
    }

    void ReturnToPool()
    {
        isFalling = false;
        myPool?.Return(this);
    }
}