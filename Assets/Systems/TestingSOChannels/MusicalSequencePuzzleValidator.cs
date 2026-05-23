using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// Listens to BoolActivatorChannel for step triggers and enforces order.
// Wrong step: resets steps, optionally replays the full correct sequence visually (without raising validation events).
public class MusicalSequencePuzzleValidator : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private BoolActivatorChannel stateChannel;

    [Header("Sequence")]
    [Tooltip("Activator IDs in solve order — must match each MusicalPuzzleStep's activatorID.")]
    [SerializeField] private List<ActivatorID> expectedOrder = new List<ActivatorID>();

    [Header("Reset targets")]
    [FormerlySerializedAs("mushrooms")]
    [Tooltip("Pieces that visually / audibly reset when the sequence fails.")]
    [SerializeField] private List<MusicalPuzzleStep> puzzleSteps = new List<MusicalPuzzleStep>();

    [Header("Failure — show correct melody")]
    [Tooltip("After a wrong tap, replay the full expected sequence: highlight each step in order.")]
    [SerializeField] private bool replayCorrectSequenceOnFailure = true;
    [SerializeField] private float correctSequenceHoldPerStepSeconds = 1f;
    [SerializeField] private float pauseBetweenReplayStepsSeconds = 0.1f;

    [Header("Completion")]
    [Tooltip("If true, after a successful solve further step events are ignored.")]
    [SerializeField] private bool disableAfterSolve = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceSolved;
    [SerializeField] private UnityEvent onSequenceReset;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private int progressIndex;
    private HashSet<ActivatorID> validIds;
    private bool puzzleComplete;
    private bool inputBlockedDuringReplay;
    private Coroutine replayRoutine;
    private readonly Dictionary<ActivatorID, MusicalPuzzleStep> stepsByActivator = new Dictionary<ActivatorID, MusicalPuzzleStep>();

    private void Awake()
    {
        validIds = new HashSet<ActivatorID>(expectedOrder);
        RebuildStepLookup();
    }

    private void OnValidate()
    {
        RebuildStepLookup();
    }

    private void RebuildStepLookup()
    {
        stepsByActivator.Clear();
        if (puzzleSteps == null) return;
        for (int i = 0; i < puzzleSteps.Count; i++)
        {
            MusicalPuzzleStep step = puzzleSteps[i];
            if (step == null) continue;
            ActivatorID id = step.ActivatorID;
            if (id == null) continue;
            if (!stepsByActivator.ContainsKey(id))
                stepsByActivator.Add(id, step);
        }
    }

    private void OnEnable()
    {
        if (stateChannel != null)
            stateChannel.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (stateChannel != null)
            stateChannel.OnStateChanged -= HandleStateChanged;
        if (replayRoutine != null)
        {
            StopCoroutine(replayRoutine);
            replayRoutine = null;
        }
        inputBlockedDuringReplay = false;
    }

    private void HandleStateChanged(ActivatorID id, bool state)
    {
        if (inputBlockedDuringReplay)
            return;

        if (puzzleComplete && disableAfterSolve)
            return;

        if (expectedOrder == null || expectedOrder.Count == 0)
            return;

        if (!validIds.Contains(id))
            return;

        ActivatorID expected = expectedOrder[progressIndex];

        if (id == expected)
        {
            progressIndex++;

            if (debugMode)
                Debug.Log($"[MusicalSequencePuzzle] Correct step: {id.name} ({progressIndex}/{expectedOrder.Count}).");

            if (progressIndex >= expectedOrder.Count)
            {
                if (debugMode)
                    Debug.Log("[MusicalSequencePuzzle] Sequence complete.");

                onSequenceSolved?.Invoke();

                if (disableAfterSolve)
                    puzzleComplete = true;
                else
                    progressIndex = 0;
            }
        }
        else
        {
            if (debugMode)
                Debug.Log($"[MusicalSequencePuzzle] Wrong order: expected '{expected.name}', got '{id.name}'. Resetting.");

            FailAndReset();
        }
    }

    private void FailAndReset()
    {
        progressIndex = 0;

        foreach (MusicalPuzzleStep step in puzzleSteps)
        {
            if (step != null)
                step.Reset();
        }

        onSequenceReset?.Invoke();

        if (replayCorrectSequenceOnFailure && expectedOrder != null && expectedOrder.Count > 0)
        {
            if (replayRoutine != null)
                StopCoroutine(replayRoutine);
            replayRoutine = StartCoroutine(ReplayCorrectSequenceRoutine());
        }
    }

    private IEnumerator ReplayCorrectSequenceRoutine()
    {
        inputBlockedDuringReplay = true;

        if (correctSequenceHoldPerStepSeconds < 0.05f)
            correctSequenceHoldPerStepSeconds = 0.05f;

        for (int i = 0; i < expectedOrder.Count; i++)
        {
            ActivatorID aid = expectedOrder[i];
            if (aid == null)
                continue;

            if (!stepsByActivator.TryGetValue(aid, out MusicalPuzzleStep piece) || piece == null)
            {
                if (debugMode)
                    Debug.LogWarning($"[MusicalSequencePuzzle] Replay: no MusicalPuzzleStep for activator '{aid.name}' — assign it in Puzzle Steps.", this);
                continue;
            }

            piece.PlayLocalInteractFeedbackOnly();
            yield return new WaitForSeconds(correctSequenceHoldPerStepSeconds);
            piece.TriggerResetPresentationOnly();
            yield return new WaitForSeconds(pauseBetweenReplayStepsSeconds);
        }

        inputBlockedDuringReplay = false;
        replayRoutine = null;
    }

    public void RearmPuzzle()
    {
        puzzleComplete = false;
        progressIndex = 0;
    }
}
