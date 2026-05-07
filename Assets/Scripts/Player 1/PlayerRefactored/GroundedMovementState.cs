public class GroundedMovementState : MovementState
{
    public override void Enter(PlayerControllerRefactored player)
    {
        // Re‑enable gravity and normal damping (original does this via exitZeroG / not needed here)
        player.rb.useGravity = true;
        player.rb.linearDamping = 0f;
        player.jumpAbility.IsEnabled = true;
        // Reset air jumps when entering grounded state (like original's OnCollisionEnter with ground)
        player.jumpAbility.ResetOnGround();
    }

    public override void Exit(PlayerControllerRefactored player)
    {
        // Nothing specific needed
    }

    public override void Tick(PlayerControllerRefactored player)
    {
        player.UpdateGroundMovementInput();
    }

    public override void FixedTick(PlayerControllerRefactored player)
    {
        player.HandleGroundMovement();
    }
}