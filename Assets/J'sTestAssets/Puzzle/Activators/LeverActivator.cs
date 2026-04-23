using UnityEngine;

/// <summary>
/// Example: How a lever activator reports its engaged state.
/// </summary>
public class LeverActivator : MonoBehaviour
{
    [SerializeField] private string leverID = "lever_1";
    private bool isEngaged = false;

    public void SetEngaged(bool engaged)
    {
        isEngaged = engaged;
        // Emit generic activator event with boolean state
        GameEvents.RaiseActivatorStateChanged(leverID, isEngaged);
    }
}
