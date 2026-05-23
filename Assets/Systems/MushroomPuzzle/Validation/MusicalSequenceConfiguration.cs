using UnityEngine;

// Author: David Haddad - CS480 design-patterns musical puzzle (May 2026)
// Configuration asset PuzzleValidator reads to know which color/note sequence solves a
// Simon-style musical puzzle (mushrooms, crystals, etc.). Pairs with MushroomSequenceTracker which re-raises the
// running sequence onto MushroomColorArrayChannel under sequenceID.

[CreateAssetMenu(fileName = "MusicalSequenceConfig", menuName = "Puzzle/Musical Sequence Configuration")]
public class MusicalSequenceConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class SequenceRequirement : IActivatorRequirement<MushroomColor[]>
    {
        [SerializeField] private ActivatorID sequenceID;
        public MushroomColor[] expectedSequence;
        public bool logComparisonChecks = true;

        public ActivatorID ActivatorID => sequenceID;

        public bool IsSatisfied(MushroomColor[] sequenceSoFar)
        {
            if (expectedSequence == null || expectedSequence.Length == 0)
            {
                if (logComparisonChecks)
                    Debug.LogWarning($"[MushroomSequenceCheck] sequenceID='{(sequenceID != null ? sequenceID.name : "null")}' has no expected sequence configured.");
                return false;
            }
            if (sequenceSoFar == null)
            {
                if (logComparisonChecks)
                    Debug.LogWarning($"[MushroomSequenceCheck] sequenceID='{(sequenceID != null ? sequenceID.name : "null")}' received null sequence.");
                return false;
            }
            if (sequenceSoFar.Length < expectedSequence.Length)
            {
                if (logComparisonChecks)
                    Debug.Log($"[MushroomSequenceCheck] sequenceID='{(sequenceID != null ? sequenceID.name : "null")}' match=False (need {expectedSequence.Length}, have {sequenceSoFar.Length}) expected=[{string.Join(", ", expectedSequence)}] current=[{string.Join(", ", sequenceSoFar)}]");
                return false;
            }

            int offset = sequenceSoFar.Length - expectedSequence.Length;
            for (int i = 0; i < expectedSequence.Length; i++)
            {
                if (sequenceSoFar[offset + i] != expectedSequence[i])
                {
                    if (logComparisonChecks)
                        Debug.Log($"[MushroomSequenceCheck] sequenceID='{(sequenceID != null ? sequenceID.name : "null")}' match=False expected=[{string.Join(", ", expectedSequence)}] comparedTail=[{BuildTailString(sequenceSoFar, offset, expectedSequence.Length)}] fullCurrent=[{string.Join(", ", sequenceSoFar)}]");
                    return false;
                }
            }

            if (logComparisonChecks)
                Debug.Log($"[MushroomSequenceCheck] sequenceID='{(sequenceID != null ? sequenceID.name : "null")}' match=True expected=[{string.Join(", ", expectedSequence)}] comparedTail=[{BuildTailString(sequenceSoFar, offset, expectedSequence.Length)}] fullCurrent=[{string.Join(", ", sequenceSoFar)}]");
            return true;
        }

        private static string BuildTailString(MushroomColor[] source, int start, int length)
        {
            if (source == null || length <= 0) return string.Empty;
            int end = Mathf.Min(source.Length, start + length);
            if (start < 0 || start >= end) return string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = start; i < end; i++)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(source[i]);
            }
            return sb.ToString();
        }
    }

    [SerializeField] private SequenceRequirement requirement;

    [Tooltip("When ON, warns once/play mode if PuzzleValidator never received MushroomColor[] for your Sequence ActivatorID (often wrong SO reference vs tracker).")]
    [SerializeField] private bool logSolveDiagnostics;

    [System.NonSerialized] private bool loggedMissingArrayStateForActivator;

    /// <summary>The ActivatorID SO raised by <see cref="MushroomSequenceTracker"/> — pair tracker + validator with this asset so it stays in one place.</summary>
    public ActivatorID SequenceActivatorID => requirement?.ActivatorID;

    /// <summary>Same melody array used by PuzzleValidator — also drives wrong-note reset on the tracker when linked.</summary>
    public MushroomColor[] ExpectedSequence => requirement != null ? requirement.expectedSequence : null;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (requirement == null || requirement.ActivatorID == null)
            return false;
        if (!state.TryGetMushroomColorArray(requirement.ActivatorID, out MushroomColor[] sequence))
        {
            if (logSolveDiagnostics && !loggedMissingArrayStateForActivator)
            {
                loggedMissingArrayStateForActivator = true;
                Debug.LogWarning(
                    $"[MusicalSequenceConfiguration:{name}] No MushroomColor[] in PuzzleValidator for ActivatorID asset '{requirement.ActivatorID.name}'. " +
                    "Use the SAME ActivatorID ScriptableObject on BOTH the melody config (Sequence ID) AND the MushroomSequenceTracker Raise (tracker Melody Configuration). Assign the same MushroomColorArrayChannel on tracker + PuzzleValidator.", this);
            }
            return false;
        }

        loggedMissingArrayStateForActivator = false;

        return requirement.IsSatisfied(sequence);
    }
}
