using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private Slider fuelSlider;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("FuelUI: no GameManager in scene.");
            return;
        }

        if (fuelText == null && fuelSlider == null)
            Debug.LogWarning("FuelUI has no Text or Slider assigned.");

        GameManager.Instance.FuelChanged += OnFuelChanged;
        OnFuelChanged(GameManager.Instance.FuelCollected, GameManager.Instance.FuelTarget);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.FuelChanged -= OnFuelChanged;
    }

    void OnFuelChanged(int collected, int target)
    {
        if (fuelText != null)
            fuelText.text = $"Fuel: {collected}/{target}";

        if (fuelSlider != null)
        {
            fuelSlider.maxValue = target;
            fuelSlider.value = collected;
        }
    }
}
