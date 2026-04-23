using UnityEngine;

public class StateListener : MonoBehaviour
{
    [SerializeField] private string listenID;
    [SerializeField] private float threshold = 0f;

    private float currentValue;

    private void OnEnable()
    {
        GameEvents.OnStoneRotationChanged += HandleState;
    }

    private void OnDisable()
    {
        GameEvents.OnStoneRotationChanged -= HandleState;
    }

    private void HandleState(string id, float value)
    {
        if (id != listenID) return;

        currentValue = value;

        Debug.Log($"{name} received value: {currentValue}");

        ApplyState();
    }

    private void ApplyState()
    {
        // Example logic:
        bool isActive = currentValue <= threshold;

        Debug.Log($"{name} active: {isActive}");

        // door / laser / platform logic here
    }
}