// =====================================================================
// Drop this on any NPC (or a child trigger collider on an NPC) to make
// them talk to the player automatically when the player walks into
// range. Configuration is per-NPC via the Inspector:
//   * Dialogue       — the conversation to play
//   * Start Channel  — shared scene-wide DialogueEventChannelSO asset
//   * End Channel    — shared scene-wide DialogueEndedChannelSO asset
//                      (used for cooldown timing only)
//   * Player Tag     — tag used to detect the player collider
//
// Reusable across any future character with no code changes.
//
// Uses the Command pattern: starting dialogue is wrapped in a
// StartDialogueCommand rather than calling startChannel.Raise() directly.
//
// Contributor: Katie Trinh — Command pattern refactor
// =====================================================================

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("Conversation that plays when the player enters this trigger.")]
    [SerializeField] private DialogueSO dialogue;

    [Header("Channels")]
    [Tooltip("Shared start channel the DialogueRunner listens to.")]
    [SerializeField] private DialogueEventChannelSO startChannel;

    [Tooltip("Shared ended channel — subscribed to so the cooldown timer " +
             "starts when the conversation finishes.")]
    [SerializeField] private DialogueEndedChannelSO endedChannel;

    [Header("Detection")]
    [Tooltip("Tag of the player collider that should activate this trigger.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("If true, the conversation only triggers when the player " +
             "physically enters the trigger volume. If false, it can also " +
             "trigger on Stay (useful if the player spawns inside).")]
    [SerializeField] private bool requireEnter = true;

    [Header("Interact Gate (opt-in)")]
    [Tooltip("When true, entering the trigger only arms the conversation — " +
             "the player must press the Interact action (E by default) to " +
             "actually start it. When false, behavior is unchanged.")]
    [SerializeField] private bool requireInteract = false;

    [Tooltip("Optional UI prompt channel. While the player is in range and " +
             "interact is required, a 'Press E to Talk'-style prompt is " +
             "raised here. Safe to leave empty.")]
    [SerializeField] private InteractionPromptChannelSO promptChannel;

    [Tooltip("Message shown by the prompt UI while the player is in range.")]
    [SerializeField] private string promptMessage = "Press E to Talk";

    [Header("First Interaction Icon")]
    [Tooltip("Optional icon above this NPC. It is hidden after the first successful interaction starts dialogue.")]
    [SerializeField] private GameObject iconToHideAfterFirstDialogue;

    // Runtime state
    private bool hasPlayedOnce = false;
    private bool isActive = false;
    private float cooldownEndsAt = 0f;
    private IDialogueCommand startCommand;
    private bool playerInRange = false;
    private Collider lastPlayerCollider;
    private bool promptShown = false;

    public bool IsPlayerInRange => playerInRange;
    public bool IsDialogueActive => isActive;
    public bool CanStartDialogue => CanPlay();

    void OnEnable()
    {
        if (endedChannel != null)
        {
            endedChannel.OnRaised += HandleDialogueEnded;
        }
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        if (endedChannel != null)
        {
            endedChannel.OnRaised -= HandleDialogueEnded;
        }
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
        HidePrompt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        lastPlayerCollider = other;

        if (requireInteract)
        {
            if (CanPlay()) ShowPrompt();
            return;
        }

        if (!requireEnter) return;
        TryStart(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!playerInRange)
        {
            playerInRange = true;
            lastPlayerCollider = other;
            if (requireInteract && CanPlay()) ShowPrompt();
        }

        if (requireInteract) return;
        if (requireEnter) return;
        TryStart(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        lastPlayerCollider = null;
        HidePrompt();
    }

    private void HandleInteractPressed()
    {
        if (!requireInteract) return;
        if (!playerInRange) return;
        if (!CanPlay()) return;
        if (lastPlayerCollider == null) return;
        HidePrompt();
        TryStart(lastPlayerCollider);
    }

    private void TryStart(Collider other)
    {
        Debug.Log($"[DialogueTrigger] Something entered '{name}': {other.name} (tag='{other.tag}')", this);
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[DialogueTrigger] Ignoring {other.name}: tag '{other.tag}' != required '{playerTag}'", this);
            return;
        }
        if (!CanPlay())
        {
            Debug.Log($"[DialogueTrigger] CanPlay() returned false (isActive={isActive}, hasPlayedOnce={hasPlayedOnce}, cooldownEndsAt={cooldownEndsAt}, time={Time.time})", this);
            return;
        }
        if (dialogue == null)
        {
            Debug.LogWarning($"[DialogueTrigger] '{name}' has no Dialogue assigned.", this);
            return;
        }
        if (startChannel == null)
        {
            Debug.LogWarning($"[DialogueTrigger] '{name}' has no Start Channel assigned.", this);
            return;
        }

        Debug.Log($"[DialogueTrigger] Raising start for dialogue '{dialogue.name}'", this);
        isActive = true;
        startCommand = new StartDialogueCommand(startChannel, dialogue);
        startCommand.Execute();
        HideFirstInteractionIcon();
    }

    private bool CanPlay()
    {
        if (isActive) return false;
        if (dialogue == null) return false;
        if (dialogue.PlayOnce && hasPlayedOnce) return false;
        if (Time.time < cooldownEndsAt) return false;
        return true;
    }

    private void HandleDialogueEnded()
    {
        // Only react if WE were the active trigger.
        if (!isActive) return;

        isActive = false;
        hasPlayedOnce = true;

        if (dialogue != null && dialogue.RetriggerCooldown > 0f)
        {
            cooldownEndsAt = Time.time + dialogue.RetriggerCooldown;
        }

        HideFirstInteractionIcon();

        // Ghost-prompt fix: if the player is still inside the trigger and
        // can play again, re-show the "Press E to Talk" prompt.
        if (requireInteract && playerInRange && CanPlay())
        {
            ShowPrompt();
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

    private void HideFirstInteractionIcon()
    {
        GameObject icon = iconToHideAfterFirstDialogue;

        if (icon == null)
        {
            DialogueIconIndicator iconIndicator = GetComponentInParent<DialogueIconIndicator>();
            if (iconIndicator == null && transform.root != null)
                iconIndicator = transform.root.GetComponentInChildren<DialogueIconIndicator>(true);

            if (iconIndicator != null)
                icon = iconIndicator.gameObject;
        }

        if (icon == null)
            return;

        icon.SetActive(false);
    }
}
