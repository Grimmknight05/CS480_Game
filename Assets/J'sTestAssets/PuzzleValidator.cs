using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PuzzleValidator : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleTrigger
    {
        public string triggerName;
        public StoneConfiguration config;
        public UnityEvent onSolved;

        [HideInInspector] public bool hasFired;
    }
    Dictionary<string, float> stoneRotations = new Dictionary<string, float>();    [SerializeField] private List<PuzzleTrigger> triggers;

    private void OnEnable()
    {
        GameEvents.OnStoneRotationChanged += HandleStoneRotation;
    }

    private void OnDisable()
    {
        GameEvents.OnStoneRotationChanged -= HandleStoneRotation;
    }
    private void HandleStoneRotation(string id, float rotation)
    {
        stoneRotations[id] = rotation;

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
    private bool IsSolved(StoneConfiguration config)
    {
        foreach (var req in config.requiredStones)
        {
            if (!stoneRotations.TryGetValue(req.stoneID, out float rotation))
                return false;

            if (!req.IsSatisfied(rotation))
                return false;
        }

        return true;
    }
}