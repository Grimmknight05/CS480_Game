using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PuzzleNode : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleEntry
    {
        public string puzzleName;
        public StoneConfig config;
        public UnityEvent onSolved;
        [HideInInspector] public bool wasSolved = false;
    }

    [SerializeField] private ActivatorStateChannelTest stateChannel;
    [SerializeField] private List<PuzzleEntry> puzzles;

    // Cache latest state of every activator
    private Dictionary<ActivatorID, float> stoneRotations = new Dictionary<ActivatorID, float>();

    private void OnEnable()
    {
        if (stateChannel != null)
            stateChannel.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (stateChannel != null)
            stateChannel.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(ActivatorID id, object state)
    {
        // Only care about float rotations (stones)
        if (state is not float rotation) return;

        stoneRotations[id] = rotation;

        // Check all puzzles
        foreach (var puzzle in puzzles)
        {
            if (puzzle.wasSolved || puzzle.config == null) continue;

            if (IsPuzzleSolved(puzzle.config))
            {
                puzzle.wasSolved = true;
                puzzle.onSolved?.Invoke();
                Debug.Log($"[PuzzleValidator] Solved: {puzzle.puzzleName}");
            }
        }
    }

    private bool IsPuzzleSolved(StoneConfig config)
    {
        foreach (var req in config.requirements)
        {
            if (!stoneRotations.TryGetValue(req.stoneID, out float rotation))
                return false; // haven't heard from this stone yet

            if (!req.IsSatisfied(rotation))
                return false;
        }
        return true;
    }
}