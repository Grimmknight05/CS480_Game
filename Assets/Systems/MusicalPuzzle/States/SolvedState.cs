using UnityEngine;

// Author: Codex update - keeps mushrooms glowing after puzzle solve.
public class SolvedState : MushroomState
{
    public override void Enter(Mushroom mushroom)
    {
        if (mushroom.LightComp != null) mushroom.LightComp.SetGlow(mushroom.GlowColor);
        if (mushroom.AudioComp != null) mushroom.AudioComp.Stop();
        if (mushroom.TimerComp != null) mushroom.TimerComp.Cancel();
    }
}
