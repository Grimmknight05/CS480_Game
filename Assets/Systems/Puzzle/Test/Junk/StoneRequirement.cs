using System;
using UnityEngine;

[Serializable]
public class StoneRequirement : IReactiveRequirement
{
    [SerializeField] private ActivatorID activatorID;
    [SerializeField] private float targetRotation;
    [SerializeField] private float tolerance;

    public ActivatorID ActivatorID => activatorID;

    public bool IsSatisfied { get; private set; }

    public event Action<IReactiveRequirement, bool> OnRequirementChanged;

    public void ProcessState(object state)
    {
        if (state is not float rotation) return;

        float diff = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));
        bool newSatisfied = diff <= tolerance;

        if (newSatisfied == IsSatisfied) return;

        IsSatisfied = newSatisfied;
        OnRequirementChanged?.Invoke(this, IsSatisfied);
    }
}