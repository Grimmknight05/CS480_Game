using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Payload broadcast through MushroomEventChannelSO when a mushroom activates.
// Listeners (critters, sequence tracker, PuzzleValidator) self-filter; puzzle color state uses PuzzleActivatorID when set.

public readonly struct MushroomActivationData
{
    public readonly string MushroomID;
    public readonly MushroomColor Color;
    public readonly MusicalNote Note;
    public readonly Vector3 Position;
    public readonly float CalmRadius;
    public readonly float Duration;
    public readonly float ActivatedAtTime;
    /// <summary>Optional: same <see cref="ActivatorID"/> as <c>MushroomConfiguration</c> requirements — drives <see cref="PuzzleValidator"/> single-color state from this one event.</summary>
    public readonly ActivatorID PuzzleActivatorID;

    public MushroomActivationData(
        string mushroomID,
        MushroomColor color,
        MusicalNote note,
        Vector3 position,
        float calmRadius,
        float duration,
        ActivatorID puzzleActivatorID = null)
    {
        MushroomID = mushroomID;
        Color = color;
        Note = note;
        Position = position;
        CalmRadius = calmRadius;
        Duration = duration;
        PuzzleActivatorID = puzzleActivatorID;
        ActivatedAtTime = Time.time;
    }
}
