using UnityEngine;

public class UseToolCommand : ICommand
{
    private ToolSystem toolSystem;
    private int toolSlotIndex;
    private float timeIssued;
    private float expiryDelay = 0.2f; // buffer window in seconds, matches JumpCommand

    public UseToolCommand(ToolSystem toolSystem, int toolSlotIndex)
    {
        this.toolSystem = toolSystem;
        this.toolSlotIndex = toolSlotIndex;
        timeIssued = Time.time;
    }

    public float ExpiryTime => timeIssued + expiryDelay;

    public bool CanExecute(PlayerControllerRefactored player)
    {
        return toolSystem.IsToolReady(toolSlotIndex);
    }

    public void Execute(PlayerControllerRefactored player)
    {
        toolSystem.UseToolBySlot(toolSlotIndex);
    }
}
