using UnityEngine;
using UnityEngine.Events;

// Based on TurnableStone pattern: raises ActivatorStateChannel when interacted.
// Pair with MusicalMushroomSequenceValidator for order-based puzzles.
public class MusicalMushroom : MonoBehaviour
{
    [Header("Event Channels")]
    [Tooltip("Channel used to notify the sequence validator (can be dedicated mushroom channel or shared).")]
    [SerializeField] private ActivatorStateChannel stateChannel;

    [Header("Identity")]
    [SerializeField] private string mushroomID;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactClip;

    [Header("Feedback")]
    [SerializeField] private UnityEvent onTriggered;
    [SerializeField] private UnityEvent onReset;

    [Header("Timing")]
    [SerializeField] private float interactCooldown = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private float nextInteractTime;

    public string MushroomID => mushroomID;

    /// <summary>
    /// Wire this from InteractableTrigger.OnInteracted (same as TurnableStone.Interact).
    /// </summary>
    public void Interact()
    {
        if (Time.time < nextInteractTime)
            return;

        nextInteractTime = Time.time + interactCooldown;

        if (debugMode)
            Debug.Log($"[MusicalMushroom] {mushroomID} triggered.");

        if (audioSource != null && interactClip != null)
            audioSource.PlayOneShot(interactClip);

        onTriggered?.Invoke();

        if (stateChannel != null)
            stateChannel.RaiseEvent(mushroomID, true);
    }

    /// <summary>
    /// Called by MusicalMushroomSequenceValidator when the player hits the wrong mushroom.
    /// Hook onReset for visuals, animator params, particles, etc.
    /// </summary>
    public void Reset()
    {
        onReset?.Invoke();

        if (debugMode)
            Debug.Log($"[MusicalMushroom] {mushroomID} reset.");
    }
}
