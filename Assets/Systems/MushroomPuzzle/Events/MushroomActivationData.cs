using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Payload broadcast through MushroomEventChannelSO when a mushroom activates.
// Listeners (critters, sequence tracker, audio mixers) self-filter on Position + CalmRadius.

public readonly struct MushroomActivationData
{
    public readonly string MushroomID;
    public readonly MushroomColor Color;
    public readonly MusicalNote Note;
    public readonly Vector3 Position;
    public readonly float CalmRadius;
    public readonly float Duration;
    public readonly float ActivatedAtTime;

    public MushroomActivationData(
        string mushroomID,
        MushroomColor color,
        MusicalNote note,
        Vector3 position,
        float calmRadius,
        float duration)
    {
        MushroomID = mushroomID;
        Color = color;
        Note = note;
        Position = position;
        CalmRadius = calmRadius;
        Duration = duration;
        ActivatedAtTime = Time.time;
    }
}
