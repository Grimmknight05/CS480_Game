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
    private TurnableStone[] trackedTurnableStones;
    private bool hasStoneConfigurationTrigger;

    private void OnEnable()
    {
        if (boolChannel != null) boolChannel.OnStateChanged += HandleBoolChanged;
        if (floatChannel != null) floatChannel.OnStateChanged += HandleFloatChanged;
        if (mushroomChannel != null) mushroomChannel.OnStateChanged += HandleMushroomColorChanged;
        if (mushroomArrayChannel != null) mushroomArrayChannel.OnStateChanged += HandleMushroomColorArrayChanged;
        CacheStoneConfigurationState();
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
        SyncTurnableStoneStates();
        for (int i = 0; i < triggers.Count; i++) triggers[i].Evaluate(this, debugMode);
    }

    private void Update()
    {
        if (!hasStoneConfigurationTrigger) return;
        if (SyncTurnableStoneStates()) CheckAllPuzzles();
    }

    private void CacheStoneConfigurationState()
    {
        hasStoneConfigurationTrigger = false;
        for (int i = 0; i < triggers.Count; i++)
        {
            if (triggers[i].config is StoneConfiguration)
            {
                hasStoneConfigurationTrigger = true;
                break;
            }
        }

        if (!hasStoneConfigurationTrigger) return;
        trackedTurnableStones = FindObjectsByType<TurnableStone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (debugMode) Debug.Log($"[PuzzleValidator] {name} tracking {trackedTurnableStones.Length} turnable stones directly.", this);
    }

    private bool SyncTurnableStoneStates()
    {
        if (!hasStoneConfigurationTrigger) return false;
        if (trackedTurnableStones == null || trackedTurnableStones.Length == 0)
        {
            trackedTurnableStones = FindObjectsByType<TurnableStone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        bool changed = false;
        for (int i = 0; i < trackedTurnableStones.Length; i++)
        {
            TurnableStone stone = trackedTurnableStones[i];
            if (stone == null || stone.ActivatorID == null) continue;
            if (stone.IsRotating) continue;
            float rotation = stone.NormalizedRotation;
            if (floatStates.TryGetValue(stone.ActivatorID, out float previous) && Mathf.Approximately(previous, rotation)) continue;
            floatStates[stone.ActivatorID] = rotation;
            changed = true;
            if (debugMode) Debug.Log($"[PuzzleValidator] {name} synced turnable stone: {stone.ActivatorID.name} = {rotation}", stone);
        }

        return changed;
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

        public void Evaluate(IPuzzleStateProvider state, bool parentDebugMode = false)
        {
            bool shouldDebug = parentDebugMode || debugMode;

            if (config == null)
            {
                if (shouldDebug) Debug.LogWarning($"[PuzzleTrigger:{triggerName}] Config is null - assign an ActivatorConfiguration.");
                return;
            }

            bool nowSolved = config.IsSolved(state);
            if (shouldDebug)
            {
                Debug.Log($"[PuzzleTrigger:{triggerName}] IsSolved={nowSolved}  wasAlreadySolved={isCurrentlySolved}  hasFired={hasFired}  reTriggerable={reTriggerable}");

                if (config is StoneConfiguration stoneConfiguration)
                {
                    Debug.Log($"[PuzzleTrigger:{triggerName}] {stoneConfiguration.GetDebugInfo(state)}");
                }
            }

            if (nowSolved == isCurrentlySolved) return;

            if (nowSolved)
            {
                if (hasFired && !reTriggerable)
                {
                    if (shouldDebug) Debug.LogWarning($"[PuzzleTrigger:{triggerName}] Blocked by hasFired+!reTriggerable - enable Re Triggerable to allow re-solve.");
                    return;
                }
                isCurrentlySolved = true;
                hasFired = true;
                if (shouldDebug) Debug.Log($"[PuzzleTrigger:{triggerName}] SOLVED. Invoking onSolved ({onSolved?.GetPersistentEventCount()} listeners).");
                onSolved?.Invoke();
            }
            else
            {
                isCurrentlySolved = false;
                if (shouldDebug) Debug.Log($"[PuzzleTrigger:{triggerName}] UNSOLVED. Invoking onUnsolved ({onUnsolved?.GetPersistentEventCount()} listeners).");
                onUnsolved?.Invoke();
            }
        }
    }
}
