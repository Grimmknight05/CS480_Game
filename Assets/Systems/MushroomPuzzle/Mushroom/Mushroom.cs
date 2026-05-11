using System.Collections.Generic;
using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Mushroom orchestrator. Composed of small components (Audio/Light/Timer/Trigger),
// owns the current MushroomState, and processes a queue of IPuzzleCommand entries
// the same way Josh's PlayerControllerRefactored processes ICommand entries.

public class Mushroom : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string mushroomID;
    [SerializeField] private MushroomColor assignedColor = MushroomColor.Green;
    [SerializeField] private MusicalNote assignedNote = MusicalNote.C;

    [Header("Activation")]
    [Tooltip("If on, after Active Duration seconds the mushroom returns to dormant. If off, it stays active until reset, wrong melody, or solve lock.")]
    [SerializeField] private bool returnToDormantAfterDuration;
    [SerializeField] private float activeDuration = 10f;
    [SerializeField] private float calmRadius = 15f;
    [SerializeField] private Color glowColor = Color.green;
    [SerializeField] private AudioClip noteClip;

    [Header("Event Channels")]
    [SerializeField] private MushroomEventChannelSO mushroomChannel;
    [SerializeField] private ActivatorStateChannel puzzleChannel;

    [Header("Components")]
    [SerializeField] private MushroomAudioComponent audioComp;
    [SerializeField] private MushroomLightComponent lightComp;
    [SerializeField] private MushroomTimerComponent timerComp;

    [Header("Command Queue")]
    [SerializeField] private float defaultCommandLifetime = 0.2f;
    [SerializeField] private PuzzleCommandHistory history;

    private MushroomState currentState;
    private readonly Queue<IPuzzleCommand> queue = new Queue<IPuzzleCommand>();
    private bool solvedLocked;

    public string MushroomID => mushroomID;
    public MushroomColor AssignedColor => assignedColor;
    public MusicalNote AssignedNote => assignedNote;
    public float ActiveDuration => activeDuration;
    public bool ReturnToDormantAfterDuration => returnToDormantAfterDuration;
    public float CalmRadius => calmRadius;
    public Color GlowColor => glowColor;
    public AudioClip NoteClip => noteClip;
    public MushroomEventChannelSO MushroomChannel => mushroomChannel;
    public ActivatorStateChannel PuzzleChannel => puzzleChannel;
    public MushroomAudioComponent AudioComp => audioComp;
    public MushroomLightComponent LightComp => lightComp;
    public MushroomTimerComponent TimerComp => timerComp;
    public MushroomState CurrentState => currentState;
    public float DefaultCommandLifetime => defaultCommandLifetime;
    public bool IsActive => currentState is ActiveState;
    public bool IsSolvedLocked => solvedLocked;
    /// <summary>True after solve lock — player triggers should not queue or run interactions.</summary>
    public bool IsInteractionLocked => solvedLocked || currentState is SolvedState;

    private void Awake()
    {
        if (audioComp == null) audioComp = GetComponentInChildren<MushroomAudioComponent>();
        if (lightComp == null) lightComp = GetComponentInChildren<MushroomLightComponent>();
        if (timerComp == null) timerComp = GetComponentInChildren<MushroomTimerComponent>();
    }

    private void Start()
    {
        SetState(new DormantState());
    }

    public void SetState(MushroomState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public void Activate()
    {
        if (solvedLocked) return;
        if (currentState is ActiveState) return;
        SetState(new ActiveState());
    }

    public void QueueCommand(IPuzzleCommand command)
    {
        if (command == null || IsInteractionLocked) return;
        queue.Enqueue(command);
    }

    public void LockSolvedGlow()
    {
        solvedLocked = true;
        queue.Clear();
        SetState(new SolvedState());
    }

    public void UnlockSolvedGlow()
    {
        solvedLocked = false;
        SetState(new DormantState());
    }

    /// <summary>Clears queued commands and returns to dormant (e.g. puzzle unsolved).</summary>
    public void ForceDormant()
    {
        solvedLocked = false;
        queue.Clear();
        SetState(new DormantState());
    }

    private void Update()
    {
        currentState?.Tick(this);
        ProcessCommandQueue();
    }

    private void ProcessCommandQueue()
    {
        while (queue.Count > 0)
        {
            var cmd = queue.Peek();
            if (Time.time > cmd.ExpiryTime)
            {
                queue.Dequeue();
                continue;
            }
            if (!cmd.CanExecute(this))
            {
                if (IsInteractionLocked)
                    queue.Dequeue();
                else
                    break;
                continue;
            }

            queue.Dequeue();
            cmd.Execute(this);
            if (history != null) history.RecordExecuted(cmd, this);
            break;
        }
    }
}
