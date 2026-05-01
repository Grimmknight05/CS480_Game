using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot / Architecture Refactor (April 2026)
// David - Updated to work with new ActivatorStateChannel and ActivatorConfiguration system (4/25/26)
public class PuzzleValidator : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleTrigger
    {
        public string triggerName;
        public ActivatorConfiguration config;
        public UnityEvent onSolved;
        public UnityEvent onUnsolved;
        public bool reTriggerable;
        [HideInInspector] public bool hasFired;
        [HideInInspector] public bool isCurrentlySolved;
    }

    [Header("Event Channels")]
    [Tooltip("The channel to listen to for stone/lever updates.")]
    [SerializeField] private ActivatorStateChannel stateChannel;

    [Header("Puzzle Configuration")]
    [SerializeField] private List<PuzzleTrigger> triggers;

    private Dictionary<string, object> activatorStates = new Dictionary<string, object>();

    private void OnEnable()
    {
        // Safe subscription to prevent memory leaks
        if (stateChannel != null)
        {
            stateChannel.OnStateChanged += HandleActivatorStateChanged;
        }
    }

    private void OnDisable()
    {
        // Safe unsubscription
        if (stateChannel != null)
        {
            stateChannel.OnStateChanged -= HandleActivatorStateChanged;
        }
    }

    // Signature updated to match the new ActivatorStateChannel (string, object)
    private void HandleActivatorStateChanged(string activatorID, object state)
    {
        activatorStates[activatorID] = state;
        CheckAllPuzzles();
    }

    private void CheckAllPuzzles()
    {
        foreach (var trigger in triggers)
        {
            if (trigger.config == null)
                continue;
                
            bool nowSolved = IsSolved(trigger.config);
            bool wasSolved = trigger.isCurrentlySolved;

            if (nowSolved && !wasSolved)//unsolved to solved
            {
                if(!trigger.reTriggerable && trigger.hasFired){continue;}
                trigger.hasFired = true;
                trigger.isCurrentlySolved = true;
                // Fire the UnityEvent to trigger environment changes
                trigger.onSolved?.Invoke();
                Debug.Log($"[PuzzleValidator] PUZZLE SOLVED: {trigger.triggerName}");
            }
            else if(!nowSolved && wasSolved)//Solved to unsolved
            {
                if(!trigger.reTriggerable && trigger.hasFired){continue;}
                trigger.isCurrentlySolved = false;
                trigger.hasFired = false;
                trigger.onUnsolved?.Invoke();
                Debug.Log($"[PuzzleValidator] PUZZLE UNSOLVED: {trigger.triggerName}");
            }
        }
    }

    private bool IsSolved(ActivatorConfiguration config)
    {
        IActivatorRequirement[] requirements = config.GetRequirements();

        foreach (var requirement in requirements)
        {
            // If we haven't heard from a required stone yet, puzzle isn't solved
            if (!activatorStates.TryGetValue(requirement.ActivatorID, out object state))
                return false;

            if (!requirement.IsSatisfied(state))
                return false;
        }

        return true;
    }
}