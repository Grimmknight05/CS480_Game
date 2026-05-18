using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleValidator : MonoBehaviour, IPuzzleStateProvider
{
    [SerializeField] private BoolActivatorChannel boolChannel;
    [SerializeField] private FloatActivatorChannel floatChannel;
    [SerializeField] private MushroomColorChannel mushroomChannel;
    [SerializeField] private MushroomColorArrayChannel mushroomArrayChannel;
    [SerializeField] private List<PuzzleTrigger> triggers = new();
    [SerializeField] private bool debugMode;

    private readonly Dictionary<ActivatorID, bool> boolStates = new();
    private readonly Dictionary<ActivatorID, float> floatStates = new();
    private readonly Dictionary<ActivatorID, MushroomColor> mushroomStates = new();
    private readonly Dictionary<ActivatorID, MushroomColor[]> mushroomArrayStates = new();

    private void OnEnable()
    {
        if (boolChannel != null) boolChannel.OnStateChanged += HandleBoolChanged;
        if (floatChannel != null) floatChannel.OnStateChanged += HandleFloatChanged;
        if (mushroomChannel != null) mushroomChannel.OnStateChanged += HandleMushroomColorChanged;
        if (mushroomArrayChannel != null) mushroomArrayChannel.OnStateChanged += HandleMushroomColorArrayChanged;
    }

    private void OnDisable()
    {
        if (boolChannel != null) boolChannel.OnStateChanged -= HandleBoolChanged;
        if (floatChannel != null) floatChannel.OnStateChanged -= HandleFloatChanged;
        if (mushroomChannel != null) mushroomChannel.OnStateChanged -= HandleMushroomColorChanged;
        if (mushroomArrayChannel != null) mushroomArrayChannel.OnStateChanged -= HandleMushroomColorArrayChanged;
    }

    private void HandleBoolChanged(ActivatorID id, bool value)
    {
        boolStates[id] = value;
        if (debugMode) Debug.Log($"[PuzzleValidator] {name} received bool: {id?.name} = {value}", this);
        CheckAllPuzzles();
    }

    private void HandleFloatChanged(ActivatorID id, float value)
    {
        floatStates[id] = value;
        if (debugMode) Debug.Log($"[PuzzleValidator] {name} received float: {id?.name} = {value}", this);
        CheckAllPuzzles();
    }

    private void HandleMushroomColorChanged(ActivatorID id, MushroomColor value)
    {
        mushroomStates[id] = value;
        CheckAllPuzzles();
    }

    private void HandleMushroomColorArrayChanged(ActivatorID id, MushroomColor[] value)
    {
        mushroomArrayStates[id] = value;
        CheckAllPuzzles();
    }

    public bool TryGetBool(ActivatorID id, out bool value) => boolStates.TryGetValue(id, out value);
    public bool TryGetFloat(ActivatorID id, out float value) => floatStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) => mushroomStates.TryGetValue(id, out value);
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) => mushroomArrayStates.TryGetValue(id, out value);

    private void CheckAllPuzzles()
    {
        for (int i = 0; i < triggers.Count; i++) triggers[i].Evaluate(this);
    }

    [Serializable]
    public class PuzzleTrigger
    {
        public string triggerName;
        public ActivatorConfiguration config;
        public UnityEvent onSolved;
        public UnityEvent onUnsolved;
        public bool reTriggerable;
        public bool debugMode;

        [NonSerialized] private bool isCurrentlySolved;
        [NonSerialized] private bool hasFired;

        public void Evaluate(IPuzzleStateProvider state)
        {
            if (config == null)
            {
                if (debugMode) Debug.LogWarning($"[PuzzleTrigger:{triggerName}] Config is null — assign an ActivatorConfiguration.");
                return;
            }

            bool nowSolved = config.IsSolved(state);
            if (debugMode) Debug.Log($"[PuzzleTrigger:{triggerName}] IsSolved={nowSolved}  wasAlreadySolved={isCurrentlySolved}  hasFired={hasFired}  reTriggerable={reTriggerable}");

            if (nowSolved == isCurrentlySolved)
            {
                if (debugMode) Debug.Log($"[PuzzleTrigger:{triggerName}] IsSolved={nowSolved} is the same as isCurrentlySolved={isCurrentlySolved} so no action is needed.");
                return;
            } 

            if (nowSolved)
            {
                if (hasFired && !reTriggerable)
                {
                    if (debugMode) Debug.LogWarning($"[PuzzleTrigger:{triggerName}] Blocked by hasFired+!reTriggerable — enable Re Triggerable to allow re-solve.");
                    return;
                }
                isCurrentlySolved = true;
                hasFired = true;
                if (debugMode) Debug.Log($"[PuzzleTrigger:{triggerName}] → SOLVED. Invoking onSolved ({onSolved?.GetPersistentEventCount()} listeners).");
                onSolved?.Invoke();
            }
            else
            {
                isCurrentlySolved = false;
                if (debugMode) Debug.Log($"[PuzzleTrigger:{triggerName}] → UNSOLVED. Invoking onUnsolved ({onUnsolved?.GetPersistentEventCount()} listeners).");
                onUnsolved?.Invoke();
            }
        }
    }
}
