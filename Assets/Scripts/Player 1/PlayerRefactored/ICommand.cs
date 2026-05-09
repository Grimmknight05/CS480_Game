using UnityEngine;
public interface ICommand
{
    float ExpiryTime { get; }
    bool CanExecute(PlayerControllerRefactored player);
    void Execute(PlayerControllerRefactored player);
}