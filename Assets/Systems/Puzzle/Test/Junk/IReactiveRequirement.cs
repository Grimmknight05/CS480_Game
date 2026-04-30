using System;

public interface IReactiveRequirement
{
    ActivatorID ActivatorID { get; }

    bool IsSatisfied { get; }

    event Action<IReactiveRequirement, bool> OnRequirementChanged;

    void ProcessState(object state);
}