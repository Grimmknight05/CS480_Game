using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private EnemyControllerTest controller;

    void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyControllerTest>();
    }

    public void TakeDamage(int amount)
    {
        if (controller != null && controller.IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (controller != null && controller.IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Enemy healed for {amount}. HP: {currentHealth}");
    }

    private void Die()
    {
        controller.Die(); // delegate to AI
    }
}