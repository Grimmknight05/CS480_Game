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

    // Runtime state
    private bool hasPlayedOnce = false;
    private bool isActive = false;
    private float cooldownEndsAt = 0f;

    void OnEnable()
    {
        if (endedChannel != null)
        {
            endedChannel.OnRaised += HandleDialogueEnded;
        }
    }

    void OnDisable()
    {
        if (endedChannel != null)
        {
            endedChannel.OnRaised -= HandleDialogueEnded;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!requireEnter) return;
        TryStart(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (requireEnter) return;
        TryStart(other);
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
        startChannel.Raise(dialogue);
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
    }
}
