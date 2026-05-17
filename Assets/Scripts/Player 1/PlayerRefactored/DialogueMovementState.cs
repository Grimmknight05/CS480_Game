using UnityEngine;

public class DialogueMovementState : MovementState
{
    public override void Enter(PlayerControllerRefactored player)
    {
        if (player.rb != null)
        {
            player.rb.linearVelocity = Vector3.zero;
        }
        if (player.Animator != null)
        {
            player.Animator.SetBool("isWalking", false);
            player.Animator.SetFloat("Speed", 0f);
        }
    }

    public override void Tick(PlayerControllerRefactored player) { }

    public override void FixedTick(PlayerControllerRefactored player) { }

    public override void Exit(PlayerControllerRefactored player) { }
}
