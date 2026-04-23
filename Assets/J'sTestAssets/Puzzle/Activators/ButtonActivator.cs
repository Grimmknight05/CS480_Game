using UnityEngine;

/// <summary>
/// Example: How a button activator reports its press count state.
/// </summary>
public class ButtonActivator : MonoBehaviour
{
    [SerializeField] private string buttonID = "button_1";
    private int pressCount = 0;

    public void OnButtonPressed()
    {
        pressCount++;
        // Emit generic activator event with int state
        GameEvents.RaiseActivatorStateChanged(buttonID, pressCount);
    }

    public void ResetPresses()
    {
        pressCount = 0;
        GameEvents.RaiseActivatorStateChanged(buttonID, pressCount);
    }
}
