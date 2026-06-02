using UnityEngine;

public class ZeroGMovementState : MovementState
{
    public override void Enter(PlayerControllerRefactored player)
    {
        if (player.CameraPivot == null && Camera.main != null)
        {
            player.CameraPivot = Camera.main.transform;
            Debug.LogWarning("ZeroGMovementState: CameraPivot was null, falling back to Main Camera.");
        }

        // If still null, log error and return (prevents crash)
        if (player.CameraPivot == null)
        {
            Debug.LogError("ZeroGMovementState: No CameraPivot available! Cannot enter state.");
            return;
        }
        player.rb.useGravity = false;
        player.rb.linearDamping = 0f;
        player.jumpAbility?.OnJumpReleased();
        if (player.jumpAbility != null)
            player.jumpAbility.IsEnabled = false;   // no jumping in zero-grav
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        player.ClearZeroGVerticalInput();
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
        player.FaceCameraDirection();
    }
}
