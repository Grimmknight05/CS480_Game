using UnityEngine;
using UnityEngine.Events;

// Author: David Haddad
// Architecture Refactor (April 25 2026)
[RequireComponent(typeof(Collider))]
public class InteractableTrigger : MonoBehaviour
{
    [Tooltip("Drag the TurnableStone.Interact method (or any other interaction) here.")]
    public UnityEvent OnInteracted;

    [Header("Prompt UI (optional)")]
    [SerializeField] private InteractionPromptChannelSO promptChannel;
    [SerializeField] private string promptMessage = "Press E to Interact";

    private bool isPlayerInRange = false;
    private bool promptShown = false;

    private void OnEnable()
    {
        // Start listening to the New Input System bridge
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        // Stop listening to prevent memory leaks
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
        HidePrompt();
    }

    private void HandleInteractPressed()
    {
        // Only fire the event if the player is actually standing inside this specific trigger zone
        if (!isPlayerInRange) return;

        // Hide the prompt while the action runs, then re-show if the player
        // is still in range (synchronous actions like a pillar rotation
        // would otherwise leave a ghost-less zone until the player walks out).
        HidePrompt();
        OnInteracted?.Invoke();
        if (isPlayerInRange) ShowPrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Assuming your player has the tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HidePrompt();
        }
    }

    private void ShowPrompt()
    {
        if (promptChannel == null) return;
        if (promptShown) return;
        promptChannel.Raise(new InteractionPromptData(this, true, promptMessage));
        promptShown = true;
    }

    private void HidePrompt()
    {
        if (promptChannel == null) return;
        if (!promptShown) return;
        promptChannel.Raise(new InteractionPromptData(this, false, string.Empty));
        promptShown = false;
    }
}
