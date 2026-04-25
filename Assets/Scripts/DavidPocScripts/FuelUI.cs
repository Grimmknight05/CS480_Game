using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private FuelStateChannel fuelState;

    void OnEnable()
    {
        if (fuelState == null)
        {
            Debug.LogWarning("FuelUI: no FuelStateChannel assigned.", this);
            return;
        }

        if (fuelText == null && fuelSlider == null)
            Debug.LogWarning("FuelUI has no Text or Slider assigned.", this);

        fuelState.OnRaised += OnFuelChanged;

        if (fuelState.HasValue)
            OnFuelChanged(fuelState.LastValue);
    }

    void OnDisable()
    {
        if (fuelState != null)
            fuelState.OnRaised -= OnFuelChanged;
    }

    void OnFuelChanged((int collected, int target) state)
    {
        var (collected, target) = state;

        if (fuelText != null)
            fuelText.text = $"Fuel: {collected}/{target}";

        if (fuelSlider != null)
        {
            fuelSlider.maxValue = target;
            fuelSlider.value = collected;
        }
    }
}
