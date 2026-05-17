using UnityEngine;

public class JumpCommand : ICommand
{
    private float timeIssued;
    private float expiryDelay = 0.2f; // buffer window in seconds
    
    public JumpCommand()
    {
        timeIssued = Time.time;
    }

    public float ExpiryTime => timeIssued + expiryDelay;

    public bool CanExecute(PlayerControllerRefactored player)
    {
        return player.jumpAbility.CanExecute(player);
    }

    public void Execute(PlayerControllerRefactored player)
    {
        player.jumpAbility.Execute(player);
    }
}