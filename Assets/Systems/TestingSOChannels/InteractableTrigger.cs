using UnityEngine;
using UnityEngine.Events;

// Author: David Haddad
// Architecture Refactor (April 25 2026)
[RequireComponent(typeof(Collider))]
public class InteractableTrigger : MonoBehaviour
{
    [Tooltip("Drag the TurnableStone.Interact method (or any other interaction) here.")]
    public UnityEvent OnInteracted;

    [Header("Activation")]
    [Tooltip("Invoke OnInteracted when the Player touches the trigger. Interact/E is not used.")]
    [SerializeField] private bool activateOnPlayerTriggerEnter;

    [Header("Debug")]
    [Tooltip("Logs trigger enter/exit, interact key handling, and when OnInteracted fires.")]
    [SerializeField] private bool diagnostics;

    [Header("Prompt UI (optional)")]
    [SerializeField] private InteractionPromptChannelSO promptChannel;
    [SerializeField] private string promptMessage = "Press E to Interact";

    private bool isPlayerInRange = false;
    private bool promptShown = false;

    private void OnEnable()
    {
        if (!activateOnPlayerTriggerEnter)
            InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        if (!activateOnPlayerTriggerEnter)
            InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
        HidePrompt();
    }

    private void HandleInteractPressed()
    {
        // Only fire the event if the player is actually standing inside this specific trigger zone
        if (diagnostics)
            Debug.Log($"[InteractableTrigger:{name}] Interact key — inRange={isPlayerInRange}", this);

        if (!isPlayerInRange) return;

        // Hide the prompt while the action runs, then re-show if the player
        // is still in range (synchronous actions like a pillar rotation
        // would otherwise leave a ghost-less zone until the player walks out).
        HidePrompt();
        InvokeOnInteracted();
        if (isPlayerInRange) ShowPrompt();
    }

    private void InvokeOnInteracted()
    {
        int n = OnInteracted != null ? OnInteracted.GetPersistentEventCount() : 0;
        if (diagnostics)
            Debug.Log($"[InteractableTrigger:{name}] Invoking OnInteracted (persistentListeners={n}).", this);

        OnInteracted?.Invoke();

        if (diagnostics && n == 0)
            Debug.LogWarning($"[InteractableTrigger:{name}] OnInteracted has no listeners — drag MusicalPuzzleStep.Interact (or TurnableStone.Interact) here.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (diagnostics)
            Debug.Log($"[InteractableTrigger:{name}] Trigger touched by '{other.name}' tag={other.tag}", this);

        // Assuming your player has the tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (activateOnPlayerTriggerEnter)
            {
                if (diagnostics)
                    Debug.Log($"[InteractableTrigger:{name}] Player entered — invoking OnInteracted (touch activation).", this);

                InvokeOnInteracted();
            }
            else
            {
                ShowPrompt();

                if (diagnostics)
                    Debug.Log($"[InteractableTrigger:{name}] Player is now IN RANGE (press Interact/E). Touch alone does not run the puzzle.", this);
            }
        }
        else if (diagnostics)
            Debug.LogWarning($"[InteractableTrigger:{name}] Non-Player touched trigger — ignoring for in-range. Expected tag Player on your character.", this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (diagnostics)
                Debug.Log($"[InteractableTrigger:{name}] Player left trigger.", this);

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
