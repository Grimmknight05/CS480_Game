using UnityEngine;

public abstract class SignalListener : MonoBehaviour
{
    [SerializeField] protected string listenID;

    protected virtual void OnEnable()
    {
        GameEvents.OnStoneRotationChanged += HandleSignal;
    }

    protected virtual void OnDisable()
    {
        GameEvents.OnStoneRotationChanged -= HandleSignal;
    }

    protected abstract void HandleSignal(string id, float value);
}