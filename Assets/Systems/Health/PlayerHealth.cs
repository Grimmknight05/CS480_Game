using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Health Regeneration")]
    [SerializeField] private float healthRegenPerSecond = 2f;
    [SerializeField] private float regenDelayAfterDamage = 3f;

    private float _timeSinceLastDamage;
    private float _regenAccumulator;

    // Events - public so other scripts can subscribe
    public UnityEvent OnHealthChanged; // Invoked when health changes
    public UnityEvent OnPlayerDeath; // Invoked when player dies
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (!IsAlive() || currentHealth >= maxHealth || healthRegenPerSecond <= 0f) return;

        _timeSinceLastDamage += Time.deltaTime;
        if (_timeSinceLastDamage < regenDelayAfterDamage) return;

        _regenAccumulator += healthRegenPerSecond * Time.deltaTime;
        int regenAmount = Mathf.FloorToInt(_regenAccumulator);
        if (regenAmount >= 1)
        {
            _regenAccumulator -= regenAmount;
            Heal(regenAmount);
        }
    }

    public void TakeDamage(int damage) //Overload IDamageable TakeDamage
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        _timeSinceLastDamage = 0f;
        _regenAccumulator = 0f;

        // Play damage sound
        if (damageSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSFX);
        }

        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount) //Overload IDamageable Heal
    {
        if (currentHealth <= 0) return; // Can't heal when dead

        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // Cap at max health

        OnHealthChanged?.Invoke();
    }

    private void Die()
    {
        if (deathSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        OnPlayerDeath?.Invoke();
        deathChannel?.Raise();
        Debug.Log("Player has died!");
    }

    public void RestoreFull()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    // Getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
}
