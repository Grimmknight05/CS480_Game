using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PuzzleValidator : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleTrigger
    {
        public string triggerName;
        public ActivatorConfiguration config; // Works with any activator type now
        public UnityEvent onSolved;

        [HideInInspector] public bool hasFired;
    }

    // Generic state dictionary: activatorID -> state (can be float, bool, int, etc)
    private Dictionary<string, object> activatorStates = new Dictionary<string, object>();

    [SerializeField] private List<PuzzleTrigger> triggers;

    private void OnEnable()
    {
        GameEvents.OnActivatorStateChanged += HandleActivatorStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnActivatorStateChanged -= HandleActivatorStateChanged;
    }

    private void HandleActivatorStateChanged(ActivatorState state)
    {
        activatorStates[state.ActivatorID] = state.State;
        CheckAllPuzzles();
    }

    private void CheckAllPuzzles()
    {
        foreach (var trigger in triggers)
        {
            if (trigger.hasFired || trigger.config == null)
                continue;

            if (IsSolved(trigger.config))
            {
                trigger.hasFired = true;
                trigger.onSolved?.Invoke();

                Debug.Log($"PUZZLE SOLVED: {trigger.triggerName}");
            }
        }
    }

    private bool IsSolved(ActivatorConfiguration config)
    {
        IActivatorRequirement[] requirements = config.GetRequirements();

        foreach (var requirement in requirements)
        {
            if (!activatorStates.TryGetValue(requirement.ActivatorID, out object state))
                return false;

            if (!requirement.IsSatisfied(state))
                return false;
        }

        return true;
    }
}