using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    // Events - public so other scripts can subscribe
    public UnityEvent OnHealthChanged; // Invoked when health changes
    public UnityEvent OnPlayerDeath; // Invoked when player dies
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;
    void Awake()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int damage)//Overload IDamageable TakeDamage
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Ensure health doesn't go below 0

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
        Debug.Log("Player has died!");
    }

    // Getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
}
