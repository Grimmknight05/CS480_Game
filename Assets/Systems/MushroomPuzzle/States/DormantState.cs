using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Inactive: restores original cap material (ClearGlow), stops audio/timer until next activation.

public class DormantState : MushroomState
{
    public override void Enter(Mushroom mushroom)
    {
        if (mushroom.LightComp != null) mushroom.LightComp.ClearGlow();
        if (mushroom.AudioComp != null) mushroom.AudioComp.Stop();
        if (mushroom.TimerComp != null) mushroom.TimerComp.Cancel();
    }
}
