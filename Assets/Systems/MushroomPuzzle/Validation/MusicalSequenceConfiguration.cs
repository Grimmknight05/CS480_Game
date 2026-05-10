using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Configuration asset PuzzleValidator reads to know which color sequence solves a
// musical-mushroom puzzle. Pairs with MushroomSequenceTracker which re-raises the
// running sequence onto ActivatorStateChannel under sequenceID.

[CreateAssetMenu(fileName = "MusicalSequenceConfig", menuName = "Puzzle/Musical Sequence Configuration")]
public class MusicalSequenceConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class SequenceRequirement : IActivatorRequirement
    {
        public string sequenceID;
        public MushroomColor[] expectedSequence;
        public bool logComparisonChecks = true;

        public string ActivatorID => sequenceID;

        public bool IsSatisfied(object activatorState)
        {
            if (expectedSequence == null || expectedSequence.Length == 0)
            {
                if (logComparisonChecks)
                    Debug.LogWarning($"[MushroomSequenceCheck] sequenceID='{sequenceID}' has no expected sequence configured.");
                return false;
            }
            if (!(activatorState is MushroomColor[] sequenceSoFar))
            {
                if (logComparisonChecks)
                    Debug.LogWarning($"[MushroomSequenceCheck] sequenceID='{sequenceID}' received invalid state type: {activatorState?.GetType().Name ?? "null"}");
                return false;
            }
            if (sequenceSoFar.Length < expectedSequence.Length)
            {
                if (logComparisonChecks)
                    Debug.Log($"[MushroomSequenceCheck] sequenceID='{sequenceID}' match=False (need {expectedSequence.Length}, have {sequenceSoFar.Length}) expected=[{string.Join(", ", expectedSequence)}] current=[{string.Join(", ", sequenceSoFar)}]");
                return false;
            }

            int offset = sequenceSoFar.Length - expectedSequence.Length;
            for (int i = 0; i < expectedSequence.Length; i++)
            {
                if (sequenceSoFar[offset + i] != expectedSequence[i])
                {
                    if (logComparisonChecks)
                        Debug.Log($"[MushroomSequenceCheck] sequenceID='{sequenceID}' match=False expected=[{string.Join(", ", expectedSequence)}] comparedTail=[{BuildTailString(sequenceSoFar, offset, expectedSequence.Length)}] fullCurrent=[{string.Join(", ", sequenceSoFar)}]");
                    return false;
                }
            }
            if (logComparisonChecks)
                Debug.Log($"[MushroomSequenceCheck] sequenceID='{sequenceID}' match=True expected=[{string.Join(", ", expectedSequence)}] comparedTail=[{BuildTailString(sequenceSoFar, offset, expectedSequence.Length)}] fullCurrent=[{string.Join(", ", sequenceSoFar)}]");
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

    /// <summary>Same ID raised by <see cref="MushroomSequenceTracker"/> — pair tracker + validator with this asset so it stays in one place.</summary>
    public string SequenceId => requirement != null ? requirement.sequenceID : string.Empty;

    /// <summary>Same melody array used by PuzzleValidator — also drives wrong-note reset on the tracker when linked.</summary>
    public MushroomColor[] ExpectedSequence => requirement != null ? requirement.expectedSequence : null;

    public override IActivatorRequirement[] GetRequirements()
    {
        return new IActivatorRequirement[] { requirement };
    }
}
