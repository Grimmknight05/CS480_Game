using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lightweight in-world fuel readout. Listens to the decoupled FuelStateChannel rather
// than reaching into a manager singleton, so it works in any scene that has the channel
// wired. Primes from the channel's replayed LastValue for late subscribers.
public class FuelUI : MonoBehaviour
{
    [SerializeField] private FuelStateChannel fuelStateChannel;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private Slider fuelSlider;

    void OnEnable()
    {
        if (fuelStateChannel == null)
        {
            Debug.LogWarning("FuelUI: no FuelStateChannel assigned.");
            return;
        }

        if (fuelText == null && fuelSlider == null)
            Debug.LogWarning("FuelUI has no Text or Slider assigned.");

        fuelStateChannel.OnRaised += OnFuelChanged;

        if (fuelStateChannel.HasValue)
        {
            FuelState state = fuelStateChannel.LastValue;
            OnFuelChanged(state);
        }
    }

    void OnDisable()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.OnRaised -= OnFuelChanged;
    }

    void OnFuelChanged(FuelState state)
    {
        if (fuelText != null)
            fuelText.text = $"Fuel: {state.collected}/{state.target}";

        if (fuelSlider != null)
        {
            fuelSlider.maxValue = state.target;
            fuelSlider.value = state.collected;
        }
    }
}
