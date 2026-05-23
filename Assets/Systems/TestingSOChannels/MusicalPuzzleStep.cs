using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// Order-based Simon / musical puzzles (mushrooms, crystals, bells, pads, etc.).
// Pair with MusicalSequencePuzzleValidator.
public class MusicalPuzzleStep : MonoBehaviour
{
    [Header("Event Channels")]
    [Tooltip("Raised only on real player steps — puzzle validation listens here.")]
    [SerializeField] private BoolActivatorChannel stateChannel;
    [SerializeField] private ActivatorID activatorID;

    [Header("Identity")]
    [FormerlySerializedAs("mushroomID")]
    [SerializeField] private string stepDisplayName;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactClip;

    [Header("Feedback")]
    [FormerlySerializedAs("onTriggered")]
    [SerializeField] private UnityEvent onPlayerStepHighlighted;
    [FormerlySerializedAs("onReset")]
    [SerializeField] private UnityEvent onResetPresentation;

    [Header("Timing")]
    [SerializeField] private float interactCooldown = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private float nextInteractTime;

    public string StepDisplayName => stepDisplayName;
    public ActivatorID ActivatorID => activatorID;

    /// <summary>
    /// Same ID string used before refactor (mushrooms, etc.).
    /// </summary>
    public string LegacyPieceId => stepDisplayName;

    /// <summary>
    /// Wire from InteractableTrigger.OnInteracted (same pattern as TurnableStone.Interact).
    /// </summary>
    public void Interact()
    {
        if (Time.time < nextInteractTime)
            return;

        nextInteractTime = Time.time + interactCooldown;

        if (debugMode)
            Debug.Log($"[MusicalPuzzleStep] '{stepDisplayName}' player step.");

        PlayLocalInteractFeedbackOnly();

        if (stateChannel != null && activatorID != null)
            stateChannel.RaiseEvent(activatorID, true);
    }

    /// <summary>
    /// Lights/sound/UI for a tap without notifying the puzzle channel (tutorial / replay).
    /// </summary>
    public void PlayLocalInteractFeedbackOnly()
    {
        if (audioSource != null && interactClip != null)
            audioSource.PlayOneShot(interactClip);

        onPlayerStepHighlighted?.Invoke();
    }

    /// <summary>
    /// Presentation-only idle state after a mistaken step reset or replay step.
    /// </summary>
    public void TriggerResetPresentationOnly()
    {
        onResetPresentation?.Invoke();

        if (debugMode)
            Debug.Log($"[MusicalPuzzleStep] '{stepDisplayName}' reset presentation.");
    }

    /// <summary>
    /// Called by MusicalSequencePuzzleValidator on wrong order (bulk reset targets).
    /// </summary>
    public void Reset()
    {
        TriggerResetPresentationOnly();
    }
}
