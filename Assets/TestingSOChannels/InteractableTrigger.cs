using UnityEngine;
using UnityEngine.Events;

// Author: David Haddad
// Architecture Refactor (April 25 2026)
[RequireComponent(typeof(Collider))]
public class InteractableTrigger : MonoBehaviour
{
    [Tooltip("Drag the TurnableStone.Interact method (or any other interaction) here.")]
    public UnityEvent OnInteracted; 

    private bool isPlayerInRange = false;

    private void OnEnable()
    {
        // Start listening to the New Input System bridge
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        // Stop listening to prevent memory leaks
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
    }

    private void HandleInteractPressed()
    {
        // Only fire the event if the player is actually standing inside this specific trigger zone
        if (isPlayerInRange)
        {
            OnInteracted?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Assuming your player has the tag "Player"
        if (other.CompareTag("Player")) 
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerInRange = false;
        }
    }
}