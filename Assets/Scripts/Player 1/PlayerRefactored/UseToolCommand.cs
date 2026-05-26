using UnityEngine;

public class UseToolCommand : ICommand
{
    private ToolSystem toolSystem;
    private Tool tool;
    private float timeIssued;
    private float expiryDelay = 0.2f;

    public UseToolCommand(ToolSystem toolSystem, Tool tool)
    {
        this.toolSystem = toolSystem;
        this.tool = tool;
        timeIssued = Time.time;
    }

    public float ExpiryTime => timeIssued + expiryDelay;

    public bool CanExecute(PlayerControllerRefactored player)
    {
        return toolSystem != null && toolSystem.IsToolReady(tool);
    }

    public void Execute(PlayerControllerRefactored player)
    {
        toolSystem.UseTool(tool);
    }
}