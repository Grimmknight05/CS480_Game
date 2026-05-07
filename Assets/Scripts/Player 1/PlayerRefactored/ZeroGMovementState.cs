using UnityEngine;

public class ZeroGMovementState : MovementState
{
    public override void Enter(PlayerControllerRefactored player)
    {
        player.rb.useGravity = false;
        player.rb.linearDamping = 0f;
        player.jumpAbility.IsEnabled = false;   // no jumping in zero‑grav
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        player.rb.useGravity = true;
        // reset horizontal velocity only (matching original exitZeroG)
        player.rb.linearVelocity = new Vector3(player.rb.linearVelocity.x, 0, player.rb.linearVelocity.z);
    }

    public override void Tick(PlayerControllerRefactored player)
    {
        player.UpdateZeroGInput();
    }

    public override void FixedTick(PlayerControllerRefactored player)
    {
        player.HandleZeroGMovement();
    }
}