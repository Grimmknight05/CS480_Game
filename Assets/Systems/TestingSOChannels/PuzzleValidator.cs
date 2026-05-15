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
        CheckAllPuzzles();
    }

    private void HandleFloatChanged(ActivatorID id, float value)
    {
        floatStates[id] = value;
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

        [NonSerialized] private bool isCurrentlySolved;
        [NonSerialized] private bool hasFired;

        public void Evaluate(IPuzzleStateProvider state)
        {
            if (config == null) return;
            bool nowSolved = config.IsSolved(state);
            if (nowSolved == isCurrentlySolved) return;

            if (nowSolved)
            {
                // Guard checked BEFORE committing state. If we can't fire, leave
                // isCurrentlySolved=false so the next unsolved→solved transition
                // is still detected correctly.
                if (hasFired && !reTriggerable) return;
                isCurrentlySolved = true;
                hasFired = true;
                onSolved?.Invoke();
            }
            else
            {
                isCurrentlySolved = false;
                onUnsolved?.Invoke();
            }
        }
    }
}
