using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Mushroom is glowing, playing its note, broadcasting to listeners. Optionally
// returns to Dormant after activeDuration when Mushroom.ReturnToDormantAfterDuration is on.

public class ActiveState : MushroomState
{
    private System.Action expiryHandler;

    public override void Enter(Mushroom mushroom)
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
            mushroom.ActiveDuration,
            mushroom.PuzzleActivatorID);

        if (mushroom.MushroomChannel != null) mushroom.MushroomChannel.Raise(data);

        if (mushroom.ReturnToDormantAfterDuration &&
            mushroom.TimerComp != null &&
            mushroom.ActiveDuration > 0f)
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
