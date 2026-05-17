using UnityEngine;

public class LeverActivator : MonoBehaviour
{
    [SerializeField] private BoolActivatorChannel stateChannel;
    [SerializeField] private ActivatorID leverID;

    private bool isEngaged;

    private void Start()
    {
        Publish();
    }

    public void SetEngaged(bool engaged)
    {
        isEngaged = engaged;
        Publish();
    }

    private void Publish()
    {
        if (stateChannel == null || leverID == null) return;
        stateChannel.RaiseEvent(leverID, isEngaged);
    }
}
