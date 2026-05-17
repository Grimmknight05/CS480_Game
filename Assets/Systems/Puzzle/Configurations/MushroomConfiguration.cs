using UnityEngine;

// Template Author: Joshua Henrikson
// Template Modified by: Sarah Temple (May 2026)
[CreateAssetMenu(fileName = "MushroomConfig", menuName = "Puzzle/Mushroom Configuration")]
public class MushroomConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class MushroomRequirement : IActivatorRequirement
    {
        [Tooltip("Must match Mushroom.MushroomID on the mushroom that raises puzzleChannel events.")]
        [SerializeField] private string mushroomID = "id_1";

        [Tooltip("If off, any activation (state is that mushroom's AssignedColor) satisfies.")]
        [SerializeField] private bool requireSpecificColor = false;

        [SerializeField] private MushroomColor expectedColor = MushroomColor.Green;

        public string ActivatorID => mushroomID;

        /// <summary>
        /// Expects the last state from ActivatorStateChannel for this ID — mushrooms raise
        /// <see cref="MushroomColor"/> via <c>Mushroom.PuzzleChannel.RaiseEvent(mushroomID, AssignedColor)</c>.
        /// </summary>
        public bool IsSatisfied(object state)
        {
            if (!(state is MushroomColor color)) return false;
            if (!requireSpecificColor) return true;
            return color == expectedColor;
        }
    }

    [SerializeField] private MushroomRequirement[] required;

    public override IActivatorRequirement[] GetRequirements() => required;
}
