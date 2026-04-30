using UnityEngine;

public abstract class BaseActivator : MonoBehaviour
{
    [SerializeField] protected ActivatorID activatorID;
    [SerializeField] protected ActivatorStateChannelTest channel;
    public ActivatorID ActivatorID => activatorID;
    protected object currentState;

    protected void SetState(object newState)
    {
        if (Equals(currentState, newState)) return;

        currentState = newState;
        channel?.Raise(activatorID, currentState);
    }
}