using UnityEngine;

// Sustained "Shift to Run" speed modifier.
// Composed onto PlayerControllerRefactored exactly like JumpAbility: the controller
// owns the instance, feeds it press/release input, and queries SpeedMultiplier when
// building target velocity. It deliberately does NOT touch rb.drag or physics
// materials -- it only scales the magnitude of the movement target.
public class SprintAbility : MovementAbility
{
    private readonly float sprintMultiplier;
    private bool isSprinting;

    public SprintAbility(float sprintMultiplier)
    {
        this.sprintMultiplier = Mathf.Max(1f, sprintMultiplier);
        IsEnabled = true;
    }

    // True only while the run key is held AND the ability is enabled.
    public bool IsSprinting => isSprinting && IsEnabled;

    // 1f when walking, sprintMultiplier when running. Consumed by the controller's
    // CurrentMoveSpeed property.
    public float SpeedMultiplier => IsSprinting ? sprintMultiplier : 1f;

    // Driven by the controller's Sprint InputAction started/canceled callbacks.
    public void SetSprinting(bool sprinting) => isSprinting = sprinting;

    public override void Enter(PlayerControllerRefactored player)
    {
        isSprinting = false;
    }
}
