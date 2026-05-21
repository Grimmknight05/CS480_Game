using UnityEngine;

public abstract class ResettableBehaviour : MonoBehaviour
{
    [SerializeField] protected LevelResetChannelSO resetChannel;
    [SerializeField] protected AreaSO area;

    protected virtual void OnEnable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised += ResetState;
    }

    protected virtual void OnDisable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised -= ResetState;
    }

    public void ResetState()
    {
        if (area != null && GameProgress.IsAreaCompleted(area))
            return;

        ResetInternal();
    }

    protected abstract void ResetInternal();
}