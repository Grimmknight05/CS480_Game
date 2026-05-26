using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any GameObject that should be damageable.
/// Implements IDamageable, fires events, plays effects, and can destroy itself.
/// </summary>
public class DamageableObject : ResettableBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Reaction")]
    [SerializeField] private bool disableColliderOnDeath = true;
    [SerializeField] private bool disableRendererOnDeath = true;

    [Header("Visual / Audio")]
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip damageSFX;

    [Header("Events")]
    public UnityEvent<int> OnDamaged;       // amount of damage taken
    public UnityEvent OnDeath;              // called when health reaches 0

    private int currentHealth;
    private bool isDead = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool wasDestroyed;
    private Collider objectCollider;
    private Renderer objectRenderer;
    private AudioSource audioSource;

    private void Awake()
    {
        currentHealth = maxHealth;
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(amount);

        // Play damage SFX
        if (damageSFX != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(damageSFX);
            else
                AudioSource.PlayClipAtPoint(damageSFX, transform.position);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    private void Die()
    {
        isDead = true;
        wasDestroyed = true;
        // Disable components to prevent further interactions
        if (disableColliderOnDeath && objectCollider != null)
            objectCollider.enabled = false;
        if (disableRendererOnDeath && objectRenderer != null)
            objectRenderer.enabled = false;

        // Effects
        if (deathVFX != null)
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        if (deathSFX != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(deathSFX);
            else
                AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        }

        OnDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }

    // Optional: reset the object for respawning (e.g., if used with ResettableBehaviour)
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        if (disableColliderOnDeath && objectCollider != null)
            objectCollider.enabled = true;
        if (disableRendererOnDeath && objectRenderer != null)
            objectRenderer.enabled = true;
    }
    protected override void ResetInternal()
    {
        // If the object was destroyed, reactivate it
        if (wasDestroyed || gameObject == null)
        {
            gameObject.SetActive(true);
            if (objectCollider != null) objectCollider.enabled = true;
            if (objectRenderer != null) objectRenderer.enabled = true;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            wasDestroyed = false;
        }
        ResetHealth();
    }

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}