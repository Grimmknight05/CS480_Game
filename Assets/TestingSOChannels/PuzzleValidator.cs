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

        [HideInInspector] public bool hasFired;
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
            if (trigger.hasFired || trigger.config == null)
                continue;

            if (IsSolved(trigger.config))
            {
                trigger.hasFired = true;
                
                // Fire the UnityEvent to trigger environment changes!
                trigger.onSolved?.Invoke();
                Debug.Log($"[PuzzleValidator] PUZZLE SOLVED: {trigger.triggerName}");
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