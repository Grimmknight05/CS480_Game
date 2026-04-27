using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthBar;

    void Start()
    {
        UpdateHealthUI();
        playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
    }

    private void UpdateHealthUI()
    {
        healthBar.value = playerHealth.GetHealthPercentage();
    }
}