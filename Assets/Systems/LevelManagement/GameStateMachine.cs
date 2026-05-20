using UnityEngine;
using System;
using System.Collections;
// Add to LevelManager or separate StateMachine
public class GameStateMachine : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.Hub;
    public event Action<GameState> OnStateChanged;

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}