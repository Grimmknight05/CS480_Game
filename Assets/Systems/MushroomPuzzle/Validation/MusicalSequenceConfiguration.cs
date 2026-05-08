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

        public string ActivatorID => sequenceID;

        public bool IsSatisfied(object activatorState)
        {
            if (expectedSequence == null || expectedSequence.Length == 0) return false;
            if (!(activatorState is MushroomColor[] sequenceSoFar)) return false;
            if (sequenceSoFar.Length < expectedSequence.Length) return false;

            int offset = sequenceSoFar.Length - expectedSequence.Length;
            for (int i = 0; i < expectedSequence.Length; i++)
            {
                if (sequenceSoFar[offset + i] != expectedSequence[i]) return false;
            }
            return true;
        }
    }

    [SerializeField] private SequenceRequirement requirement;

    public override IActivatorRequirement[] GetRequirements()
    {
        return new IActivatorRequirement[] { requirement };
    }
}
