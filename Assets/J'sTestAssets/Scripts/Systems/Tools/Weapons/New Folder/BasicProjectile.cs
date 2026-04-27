using UnityEngine;

public class BasicProjectile : MonoBehaviour, IProjectile
{
    private Rigidbody rb;
    private int damage;
    private Vector3 moveDirection;
    private float lifetime = 10f;
    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        // Move projectile
        rb.linearVelocity = moveDirection;

        // Track lifetime
        timer += Time.fixedDeltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector3 direction, int damageAmount, Transform sourceTransform)
    {
        moveDirection = direction;
        damage = damageAmount;
        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    public void Explode()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyControllerTest enemy = collision.GetComponent<EnemyControllerTest>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[BasicProjectile] Dealt {damage} damage!");
                Explode();
            }
        }
    }
}
