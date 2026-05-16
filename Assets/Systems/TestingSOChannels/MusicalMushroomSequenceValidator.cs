using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Listens to BoolActivatorChannel for mushroom triggers and enforces order.
// Wrong step: progress resets to zero and all referenced MusicalMushrooms get Reset().
public class MusicalMushroomSequenceValidator : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private BoolActivatorChannel stateChannel;

    [Header("Sequence")]
    [Tooltip("ActivatorID SOs in the exact order the player must trigger them (must match each MusicalMushroom's activatorID field).")]
    [SerializeField] private List<ActivatorID> expectedOrder = new List<ActivatorID>();

    [Header("Reset targets")]
    [Tooltip("All mushrooms that should visually / audibly reset when the sequence fails.")]
    [SerializeField] private List<MusicalMushroom> mushrooms = new List<MusicalMushroom>();

    [Header("Completion")]
    [Tooltip("If true, after a successful solve further mushroom events are ignored.")]
    [SerializeField] private bool disableAfterSolve = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceSolved;
    [SerializeField] private UnityEvent onSequenceReset;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private int progressIndex;
    private HashSet<ActivatorID> validIds;
    private bool puzzleComplete;

    private void Awake()
    {
        validIds = new HashSet<ActivatorID>(expectedOrder);
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
    }

    private void HandleStateChanged(ActivatorID id, bool state)
    {
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
                Debug.Log($"[MushroomSequence] Correct step: {id.name} ({progressIndex}/{expectedOrder.Count}).");

            if (progressIndex >= expectedOrder.Count)
            {
                if (debugMode)
                    Debug.Log("[MushroomSequence] Sequence complete.");

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
                Debug.Log($"[MushroomSequence] Wrong order: expected '{expected.name}', got '{id.name}'. Restarting sequence.");

            FailAndReset();
        }
    }

    private void FailAndReset()
    {
        progressIndex = 0;

        foreach (MusicalMushroom m in mushrooms)
        {
            if (m != null)
                m.Reset();
        }

        onSequenceReset?.Invoke();
    }

    public void RearmPuzzle()
    {
        puzzleComplete = false;
        progressIndex = 0;
    }
}
