using System.Collections.Generic;
using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Bridges per-mushroom activations to the existing puzzle framework. Subscribes
// to MushroomEventChannelSO (one broadcast: critters, tracker, validator), accumulates
// a bounded list of recent colors, and re-raises the running sequence on MushroomColorArrayChannel
// so MushroomPuzzleValidator (or PuzzleValidator) can match a MusicalSequenceConfiguration with the same Sequence Activator ID.
// Edited by Sarah Using Cursor

public class MushroomSequenceTracker : MonoBehaviour
{
    [Header("Melody (single source of truth)")]
    [Tooltip("Same asset as the puzzle root's trigger Config — defines Sequence ID + Expected Sequence once.")]
    [SerializeField] private MusicalSequenceConfiguration melodyConfiguration;

    [SerializeField] private MushroomEventChannelSO mushroomChannel;
    [SerializeField] private MushroomColorArrayChannel puzzleChannel;
    [SerializeField] private int maxHistory = 20;
    [SerializeField] private bool logHistoryChannelToConsole = true;
    [Tooltip("Logs solve chain: tracker → channel RaiseEvent → (expect) MushroomPuzzleValidator Evaluate → OnPuzzleSolved. Enable while troubleshooting.")]
    [SerializeField] private bool logPuzzleSolveDebug;

    [Header("Progress reset")]
    [Tooltip("Played when this tracker clears progress (Clear(), or wrong note if auto-reset is on).")]
    [SerializeField] private AudioClip progressResetClip;
    [SerializeField] [Range(0f, 1f)] private float resetSoundVolume = 1f;
    [Tooltip("Uses Expected Sequence from Melody Configuration. Resets only on a wrong tap — partial progress never resets. On by default so wrong notes clear the run.")]
    [SerializeField] private bool resetWhenSequenceBreaksExpectedPrefix = true;
    [Tooltip("Logs when a wrong tap triggers a reset (and one-time warnings if setup is incomplete).")]
    [SerializeField] private bool logWrongNoteResetDiagnostics;
    [Header("Solved behavior")]
    [SerializeField] private bool lockMushroomsGlowingWhenSolved = true;
    [Tooltip("All mushrooms in this puzzle — used for solve lock, wrong-note reset (force dormant), and optional unsolve reset.")]
    [SerializeField] private Mushroom[] mushroomsToLockOnSolved;
    [SerializeField] private AudioClip solvedClip;
    [SerializeField] [Range(0f, 1f)] private float solvedClipVolume = 0.8f;
    [Tooltip("Walks to Rest Point Under Mushroom OR plain enemy pacify: assign critter/enemy roots. Critters use PacifyAfterPuzzleSolve; enemies use SetPacifiedCombat.")]
    [SerializeField] private GameObject[] pacifyOnSolve;

    [Tooltip("Obsolete — migrated at runtime into Pacify On Solve if that list is empty. Remove after re-saving.")]
    [SerializeField, HideInInspector] private CritterController[] crittersToPacifyOnSolve;

    [Tooltip("Obsolete — migrated at runtime into Pacify On Solve if that list is empty. Remove after re-saving.")]
    [SerializeField, HideInInspector] private EnemyControllerTest[] enemiesToPacifyOnSolve;
    [Tooltip("Opened when puzzle solves (same timing as Mushroom Puzzle Validator trigger On Solved → OnPuzzleSolved). Uses DoorLerp.Open().")]
    [SerializeField] private DoorLerp[] doorsToOpenOnSolve;

    [Header("Unsolve reset (optional)")]
    [Tooltip("Off by default. Wrong-note reset uses only 'Reset When Sequence Breaks Expected Prefix' above. Enable this only if you need full reset when the puzzle validator fires On Unsolved (e.g. was solved, then state invalidates).")]
    [SerializeField] private bool applyResetOnPuzzleUnsolve;
    [Tooltip("Shared command log — cleared only when Apply Reset On Puzzle Unsolve is enabled.")]
    [SerializeField] private PuzzleCommandHistory puzzleCommandHistory;

    private readonly List<MushroomColor> history = new List<MushroomColor>();

    private void Awake()
    {
        if (melodyConfiguration == null)
            Debug.LogWarning("[MushroomSequenceTracker] Assign the same Musical Sequence Configuration asset used by Mushroom Puzzle Validator trigger (sequence ID + melody in one place).");
        if (mushroomsToLockOnSolved == null || mushroomsToLockOnSolved.Length == 0)
            mushroomsToLockOnSolved = GetComponentsInChildren<Mushroom>(true);
        if (puzzleCommandHistory == null)
            puzzleCommandHistory = GetComponentInChildren<PuzzleCommandHistory>(true);

        ValidateWrongNoteResetSetup();
        LogSolveDebugSetupOnce();
        MigrateLegacyPacifyAssignments();
    }

    private void MigrateLegacyPacifyAssignments()
    {
        bool hasPacify = pacifyOnSolve != null && pacifyOnSolve.Length > 0;
        int nc = crittersToPacifyOnSolve != null ? crittersToPacifyOnSolve.Length : 0;
        int ne = enemiesToPacifyOnSolve != null ? enemiesToPacifyOnSolve.Length : 0;
        if (hasPacify || (nc == 0 && ne == 0))
            return;

        var merged = new List<GameObject>();
        for (int i = 0; i < nc; i++)
            if (crittersToPacifyOnSolve[i] != null)
                merged.Add(crittersToPacifyOnSolve[i].gameObject);
        for (int i = 0; i < ne; i++)
            if (enemiesToPacifyOnSolve[i] != null)
                merged.Add(enemiesToPacifyOnSolve[i].gameObject);
        pacifyOnSolve = merged.ToArray();
    }

    private void LogSolveDebugSetupOnce()
    {
        if (!logPuzzleSolveDebug) return;
        ActivatorID aid = melodyConfiguration != null ? melodyConfiguration.SequenceActivatorID : null;
        int expLen = melodyConfiguration?.ExpectedSequence != null ? melodyConfiguration.ExpectedSequence.Length : 0;
        Debug.Log(
            $"[MushroomSequenceTracker:{name}] Solve-debug setup.\n" +
            $"  MushroomChannel: {(mushroomChannel != null ? mushroomChannel.name : "MISSING — mushrooms never reach tracker")}\n" +
            $"  Mushroom snapshot channel (array): {(puzzleChannel != null ? puzzleChannel.name : "MISSING — validator never hears melody")}\n" +
            $"  Melody: {(melodyConfiguration != null ? melodyConfiguration.name : "MISSING")}\n" +
            $"  Sequence Activator ID: {(aid != null ? aid.name + " (instance OK if same asset ref as validator melody)" : "MISSING — open melody asset, assign Sequence ID")}\n" +
            $"  Expected melody length: {expLen}");
    }

    private void ValidateWrongNoteResetSetup()
    {
        if (!resetWhenSequenceBreaksExpectedPrefix)
            return;
        if (melodyConfiguration == null)
        {
            Debug.LogWarning($"[MushroomSequenceTracker] {name}: Wrong-note reset is ON but Melody Configuration is not assigned.");
            return;
        }
        var exp = melodyConfiguration.ExpectedSequence;
        if (exp == null || exp.Length == 0)
        {
            Debug.LogWarning($"[MushroomSequenceTracker] {name}: Wrong-note reset is ON but '{melodyConfiguration.name}' has no Expected Sequence (open the asset → requirement → expected Sequence).");
        }
    }

    private void OnEnable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised += OnMushroomActivated;
    }

    private void OnDisable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised -= OnMushroomActivated;
    }

    private void OnMushroomActivated(MushroomActivationData data)
    {
        history.Add(data.Color);
        if (history.Count > maxHistory) history.RemoveAt(0);

        MushroomColor[] expected = melodyConfiguration != null ? melodyConfiguration.ExpectedSequence : null;

        // Wrong-note reset only: partial progress (correct prefix, fewer notes than goal) stays valid.
        if (resetWhenSequenceBreaksExpectedPrefix &&
            expected != null &&
            expected.Length > 0 &&
            !IsValidPartialOrCompletePrefix(history, expected))
        {
            if (logWrongNoteResetDiagnostics)
                Debug.Log($"[MushroomSequenceTracker] Wrong tap — resetting. History was: [{string.Join(", ", history)}], goal: [{string.Join(", ", expected)}]");
            ResetProgressInternal(playSound: true);
            return;
        }

        RaiseSnapshotToChannel(history.ToArray());
    }

    /// <summary>Clears stored colors, notifies the puzzle channel, and plays <see cref="progressResetClip"/> if assigned.</summary>
    public void Clear()
    {
        ResetProgressInternal(playSound: history.Count > 0);
    }

    private void ResetProgressInternal(bool playSound)
    {
        bool hadEntries = history.Count > 0;
        history.Clear();
        ForceAllPuzzleMushroomsDormant();
        if (puzzleCommandHistory != null)
            puzzleCommandHistory.ClearAll();
        RaiseSnapshotToChannel(System.Array.Empty<MushroomColor>());
        if (playSound && hadEntries)
            PlayProgressResetSound();
    }

    private void ForceAllPuzzleMushroomsDormant()
    {
        if (mushroomsToLockOnSolved == null) return;
        for (int i = 0; i < mushroomsToLockOnSolved.Length; i++)
        {
            if (mushroomsToLockOnSolved[i] == null) continue;
            mushroomsToLockOnSolved[i].ForceDormant();
        }
    }

    private void RaiseSnapshotToChannel(MushroomColor[] snapshot)
    {
        if (puzzleChannel == null)
        {
            if (logPuzzleSolveDebug)
                Debug.LogWarning($"[MushroomSequenceTracker:{name}] RaiseSnapshot aborted: puzzleChannel (MushroomColorArrayChannel) not assigned.", this);
            return;
        }

        ActivatorID id = melodyConfiguration != null ? melodyConfiguration.SequenceActivatorID : null;
        if (id == null)
        {
            Debug.LogWarning("[MushroomSequenceTracker] Assign Melody Configuration (same asset as the puzzle validator trigger Config) so Sequence Activator ID is set.");
            return;
        }

        puzzleChannel.RaiseEvent(id, snapshot);

        MushroomColor[] expected = melodyConfiguration != null ? melodyConfiguration.ExpectedSequence : null;
        int L = expected != null ? expected.Length : 0;
        if (logPuzzleSolveDebug && L > 0 && snapshot != null && snapshot.Length == L)
        {
            bool tailOk = IsValidPartialOrCompletePrefix(new List<MushroomColor>(snapshot), expected);
            Debug.Log($"[MushroomSequenceTracker:{name}] Emitted FULL-LENGTH snapshot ({snapshot.Length} notes); tail matches melody = {tailOk}. Validator should run trigger IsSolved now.", this);
        }

        if (logHistoryChannelToConsole || logPuzzleSolveDebug)
        {
            Debug.Log($"[MushroomSequenceTracker] Raised '{id.name}' snapshot len={snapshot?.Length ?? 0}: {(snapshot != null && snapshot.Length > 0 ? string.Join(", ", snapshot) : "(empty)")}", this);
        }
    }

    private void PlayProgressResetSound()
    {
        if (progressResetClip == null) return;
        AudioSource.PlayClipAtPoint(progressResetClip, transform.position, resetSoundVolume);
    }

    /// <summary>
    /// While the melody is incomplete (fewer taps than the goal), only a strict prefix must match — never reset for "not enough notes yet."
    /// After the list is longer than the goal, use the same suffix window as <see cref="MusicalSequenceConfiguration"/>.
    /// </summary>
    private static bool IsValidPartialOrCompletePrefix(List<MushroomColor> hist, MushroomColor[] expected)
    {
        int n = hist.Count;
        int L = expected.Length;
        if (n == 0) return true;

        if (n <= L)
        {
            for (int i = 0; i < n; i++)
            {
                if (hist[i] != expected[i])
                    return false;
            }
            return true;
        }

        int offset = n - L;
        for (int i = 0; i < L; i++)
        {
            if (hist[offset + i] != expected[i])
                return false;
        }
        return true;
    }

    public void OnPuzzleSolved()
    {
        if (logPuzzleSolveDebug)
        {
            int doorN = doorsToOpenOnSolve != null ? doorsToOpenOnSolve.Length : 0;
            Debug.Log($"[MushroomSequenceTracker:{name}] ** OnPuzzleSolved ** (doors={doorN}, pacifyRoots={pacifyOnSolve?.Length ?? 0}, lockGlow={lockMushroomsGlowingWhenSolved}).", this);
        }

        if (solvedClip != null)
            AudioSource.PlayClipAtPoint(solvedClip, transform.position, solvedClipVolume);

        if (pacifyOnSolve != null)
        {
            for (int i = 0; i < pacifyOnSolve.Length; i++)
            {
                GameObject go = pacifyOnSolve[i];
                if (go == null) continue;
                CritterController critter = go.GetComponent<CritterController>();
                if (critter != null)
                {
                    critter.PacifyAfterPuzzleSolve();
                    continue;
                }

                EnemyControllerTest enemy = go.GetComponent<EnemyControllerTest>();
                if (enemy == null) enemy = go.GetComponentInChildren<EnemyControllerTest>(true);
                enemy?.SetPacifiedCombat();
            }
        }

        if (doorsToOpenOnSolve != null)
        {
            for (int i = 0; i < doorsToOpenOnSolve.Length; i++)
                doorsToOpenOnSolve[i]?.Open();
        }

        if (!lockMushroomsGlowingWhenSolved || mushroomsToLockOnSolved == null) return;
        for (int i = 0; i < mushroomsToLockOnSolved.Length; i++)
        {
            if (mushroomsToLockOnSolved[i] == null) continue;
            mushroomsToLockOnSolved[i].LockSolvedGlow();
        }
    }

    public void OnPuzzleUnsolved()
    {
        if (!applyResetOnPuzzleUnsolve)
            return;

        if (mushroomsToLockOnSolved != null)
        {
            for (int i = 0; i < mushroomsToLockOnSolved.Length; i++)
            {
                if (mushroomsToLockOnSolved[i] == null) continue;
                mushroomsToLockOnSolved[i].ForceDormant();
            }
        }

        if (puzzleCommandHistory != null)
            puzzleCommandHistory.ClearAll();

        ResetProgressInternal(playSound: false);
    }
}
