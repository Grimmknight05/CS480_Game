using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Mushroom is glowing, playing its note, broadcasting to listeners. Holds for
// mushroom.activeDuration via the timer component, then flips back to Dormant.

public class ActiveState : MushroomState
{
    private System.Action expiryHandler;

    public override void Enter(Mushroom mushroom)
    {
        ApplyActivation(mushroom);
    }

    /// <summary>
    /// Same meaning as entering ActiveState, but without swapping state — used when the player
    /// hits the same mushroom again before it goes dormant (melodies like green → yellow → green).
    /// </summary>
    public void Retrigger(Mushroom mushroom)
    {
        ApplyActivation(mushroom);
    }

    private void ApplyActivation(Mushroom mushroom)
    {
        if (mushroom.LightComp != null) mushroom.LightComp.SetGlow(mushroom.GlowColor);
        if (mushroom.AudioComp != null) mushroom.AudioComp.PlayNote(mushroom.NoteClip, mushroom.ActiveDuration);

        var data = new MushroomActivationData(
            mushroom.MushroomID,
            mushroom.AssignedColor,
            mushroom.AssignedNote,
            mushroom.transform.position,
            mushroom.CalmRadius,
            mushroom.ActiveDuration);

        if (mushroom.MushroomChannel != null) mushroom.MushroomChannel.Raise(data);
        if (mushroom.PuzzleChannel != null) mushroom.PuzzleChannel.RaiseEvent(mushroom.MushroomID, mushroom.AssignedColor);

        if (mushroom.TimerComp != null)
        {
            if (expiryHandler != null)
            {
                mushroom.TimerComp.OnExpired -= expiryHandler;
                expiryHandler = null;
            }

            expiryHandler = () => mushroom.SetState(new DormantState());
            mushroom.TimerComp.OnExpired += expiryHandler;
            mushroom.TimerComp.StartTimer(mushroom.ActiveDuration);
        }
    }

    public override void Exit(Mushroom mushroom)
    {
        if (mushroom.TimerComp != null && expiryHandler != null)
        {
            mushroom.TimerComp.OnExpired -= expiryHandler;
            expiryHandler = null;
        }
    }
}
