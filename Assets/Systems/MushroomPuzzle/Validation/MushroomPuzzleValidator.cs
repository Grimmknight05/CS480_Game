using System.Collections.Generic;
using UnityEngine;


public class MushroomPuzzleValidator : MonoBehaviour, IPuzzleStateProvider
{
    [Tooltip("Same Mushroom Event Channel asset as your Mushroom prefabs. Feeds per-mushroom color into Mushroom Configuration triggers (via Puzzle Activator ID on each mushroom).")]
    [SerializeField] private MushroomEventChannelSO mushroomChannel;
    [SerializeField] private MushroomColorArrayChannel mushroomArrayChannel;
    [SerializeField] private List<PuzzleValidator.PuzzleTrigger> triggers = new();
    [Tooltip("Verbose logs for mushroom channel/trigger evaluation. Also enable Mushroom Sequence Tracker ▸ Log Puzzle Solve Debug for melody-specific setup.")]
    [SerializeField] private bool debugMode;

    private readonly Dictionary<ActivatorID, MushroomColor> mushroomStates = new();
    private readonly Dictionary<ActivatorID, MushroomColor[]> mushroomArrayStates = new();

    private void Start()
    {
        bool usesMelody = triggers.Exists(t => t.config is MusicalSequenceConfiguration);
        if (usesMelody && mushroomArrayChannel == null)
            Debug.LogWarning($"[{nameof(MushroomPuzzleValidator)}:{name}] Trigger uses Musical Sequence Configuration but Mushroom Color Array Channel is not assigned — melody solves will not register.", this);

        bool usesMushroomConfig = triggers.Exists(t => t.config is MushroomConfiguration);
        if (usesMushroomConfig && mushroomChannel == null)
            Debug.LogWarning($"[{nameof(MushroomPuzzleValidator)}:{name}] Trigger uses Mushroom Configuration but Mushroom Channel is not assigned — assign the same Mushroom Event Channel SO as on your mushrooms.", this);
    }

    private void OnEnable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised += HandleMushroomActivation;
        if (mushroomArrayChannel != null) mushroomArrayChannel.OnStateChanged += HandleMushroomColorArrayChanged;
    }

    private void OnDisable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised -= HandleMushroomActivation;
        if (mushroomArrayChannel != null) mushroomArrayChannel.OnStateChanged -= HandleMushroomColorArrayChanged;
    }

    private void HandleMushroomActivation(MushroomActivationData data)
    {
        if (data.PuzzleActivatorID != null)
            mushroomStates[data.PuzzleActivatorID] = data.Color;
        if (debugMode)
            Debug.Log($"[{nameof(MushroomPuzzleValidator)}:{name}] Mushroom activation id='{data.MushroomID}' color={data.Color} puzzleActivator={(data.PuzzleActivatorID != null ? data.PuzzleActivatorID.name : "none")}", this);
        CheckAllPuzzles();
    }

    private void HandleMushroomColorArrayChanged(ActivatorID id, MushroomColor[] value)
    {
        mushroomArrayStates[id] = value;
        if (debugMode)
            Debug.Log($"[{nameof(MushroomPuzzleValidator)}:{name}] Array update id='{(id != null ? id.name : "?")}' len={(value != null ? value.Length : -1)}", this);
        CheckAllPuzzles();
    }

    public bool TryGetBool(ActivatorID id, out bool value)
    {
        value = false;
        return false;
    }

    public bool TryGetFloat(ActivatorID id, out float value)
    {
        value = 0f;
        return false;
    }

    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) => mushroomStates.TryGetValue(id, out value);
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) => mushroomArrayStates.TryGetValue(id, out value);

    private void CheckAllPuzzles()
    {
        for (int i = 0; i < triggers.Count; i++) triggers[i].Evaluate(this);
    }
}
