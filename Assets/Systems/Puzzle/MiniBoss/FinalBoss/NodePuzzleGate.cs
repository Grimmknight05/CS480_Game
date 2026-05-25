using System.Collections.Generic;
using UnityEngine;

public class NodePuzzleGate : MonoBehaviour, IPuzzleStateProvider
{
    [Header("Stone Configuration")]
    [SerializeField] private FloatActivatorChannel stoneStateChannel;
    
    [SerializeField] private ActivatorID stone1ID;
    [SerializeField] private ActivatorID stone2ID;
    [SerializeField] private ActivatorID stone3ID;
    
    [Header("Phase Reference")]
    [SerializeField] private NodeDestructionPhase nodePhase;
    
    private readonly Dictionary<ActivatorID, float> stoneStates = new();
    private bool puzzleSolved = false;
    
    private void OnEnable()
    {
        if (stoneStateChannel != null)
        {
            stoneStateChannel.OnStateChanged += OnStoneStateChanged;
        }
    }
    
    private void OnDisable()
    {
        if (stoneStateChannel != null)
        {
            stoneStateChannel.OnStateChanged -= OnStoneStateChanged;
        }
    }
    
    private void OnStoneStateChanged(ActivatorID id, float value)
    {
        stoneStates[id] = value;
        CheckPuzzleState();
    }
    
    private void CheckPuzzleState()
    {
        if (puzzleSolved) return;
        
        bool stone1Solved = stoneStates.TryGetValue(stone1ID, out float val1) && Mathf.Approximately(val1, 0f);
        bool stone2Solved = stoneStates.TryGetValue(stone2ID, out float val2) && Mathf.Approximately(val2, 0f);
        bool stone3Solved = stoneStates.TryGetValue(stone3ID, out float val3) && Mathf.Approximately(val3, 0f);
        
        if (stone1Solved && stone2Solved && stone3Solved)
        {
            SolvePuzzle();
        }
    }
    
    private void SolvePuzzle()
    {
        if (puzzleSolved) return;
        
        puzzleSolved = true;
        Debug.Log("[NodePuzzleGate] All 3 stones solved! Signaling NodeDestructionPhase.");
        
        if (nodePhase != null)
        {
            nodePhase.OnPuzzleSolved();
        }
    }
    
    public bool TryGetBool(ActivatorID id, out bool value) { value = default; return false; }
    public bool TryGetFloat(ActivatorID id, out float value) => stoneStates.TryGetValue(id, out value);
    public bool TryGetMushroomColor(ActivatorID id, out MushroomColor value) { value = default; return false; }
    public bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value) { value = default; return false; }
}
