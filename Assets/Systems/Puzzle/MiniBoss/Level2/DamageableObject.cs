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

    [Header("Shield")]
    [SerializeField] private bool startWithShield = true;
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private float shieldRotationSpeed = 90f;

    [Header("Events")]
    public UnityEvent<int> OnDamaged;
    public UnityEvent OnDeath;

    private bool isInvulnerable = false;
    private bool isShielded = false;
    private int currentHealth;
    private bool isDead = false;

    // Reset state
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool wasDestroyed;
    private bool wasInvulnerable;
    private bool wasShielded;

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
        wasInvulnerable = isInvulnerable;
        wasShielded = startWithShield;

        if (startWithShield)
            SetShieldActive(true);
    }

    private void Update()
    {
        if (isShielded && shieldVisual != null)
        {
            shieldVisual.transform.Rotate(Vector3.up, shieldRotationSpeed * Time.deltaTime);
        }
    }

    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    public void SetShieldActive(bool active)
    {
        isShielded = active;
        if (shieldVisual != null)
            shieldVisual.SetActive(active);
    }
    public void ResetForPool()
    {
        ResetHealth();
        wasDestroyed = false;
        SetShieldActive(startWithShield);
    }
    public void TakeDamage(int amount)
    {
        if (isDead || isInvulnerable) return;
        if (isShielded) return;  // Shield blocks all damage
        if (amount <= 0) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(amount);

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

        if (disableColliderOnDeath && objectCollider != null)
            objectCollider.enabled = false;
        if (disableRendererOnDeath && objectRenderer != null)
            objectRenderer.enabled = false;

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
        // Reactivate if destroyed
        if (wasDestroyed)
        {
            gameObject.SetActive(true);
            if (objectCollider != null) objectCollider.enabled = true;
            if (objectRenderer != null) objectRenderer.enabled = true;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            wasDestroyed = false;
        }

        // Restore invulnerability and shield states
        isInvulnerable = wasInvulnerable;
        if (wasShielded)
            SetShieldActive(true);
        else
            SetShieldActive(false);

        ResetHealth();
    }

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}   