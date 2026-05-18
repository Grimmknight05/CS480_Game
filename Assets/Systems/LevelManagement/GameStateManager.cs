using UnityEngine;

public abstract class GameState
{
    protected GameStateManager   manager;
    public GameState(GameStateManager mgr) => manager = mgr;
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }
}

public class HubState : GameState
{
    public HubState(GameStateManager mgr) : base(mgr) { }
    public override void OnEnter()
    {
        // Enable hub UI, disable player death respawn?
    }
}

public class LevelState : GameState
{
    public LevelState(GameStateManager mgr) : base(mgr) { }
    public override void OnEnter()
    {
        // Enable level HUD, ensure checkpoint system active
    }
}

public class GameStateManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    public void ChangeState(GameState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

    void Update()
    {
        CurrentState?.OnUpdate();
    }
}