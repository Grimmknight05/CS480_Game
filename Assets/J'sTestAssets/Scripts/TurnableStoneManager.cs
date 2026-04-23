using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TurnableStoneManager : MonoBehaviour
{

    
    [System.Serializable]
    public class StoneEventTrigger //Trigger event
    {
        public string triggerName;
        public StoneConfiguration stoneConfig;
        public UnityEvent onPuzzleSolved = new UnityEvent();
        
        [HideInInspector] public bool hasBeenTriggered = false;
    }
    //Event trigger list
    [SerializeField] private List<StoneEventTrigger> eventTriggers = new List<StoneEventTrigger>();    
    private Dictionary<string, TurnableStone> stoneLookup; //Registered Stones Dict
    
    private void Awake()
    {
        stoneLookup = new Dictionary<string, TurnableStone>();

        var stones = FindObjectsByType<TurnableStone>(FindObjectsSortMode.None);

        foreach (var stone in stones)
        {
            if (string.IsNullOrEmpty(stone.StoneID))
                continue;

            stoneLookup[stone.StoneID] = stone;

            // IMPORTANT: subscribe to event
            stone.OnReachedTarget += HandleStoneChanged;
        }
    }
    private void HandleStoneChanged(TurnableStone stone)
    {
        
        Debug.Log($"Stone changed: {stone.StoneID}");

        CheckAllTriggers();
    }
    private void OnDestroy()
    {
        foreach (var stone in stoneLookup.Values)
        {
            stone.OnReachedTarget -= HandleStoneChanged;
        }
    }
    public void RegisterStone(TurnableStone stone)
    {
        if (stone == null || string.IsNullOrEmpty(stone.StoneID))
            return;

        if (!stoneLookup.ContainsKey(stone.StoneID))
            stoneLookup.Add(stone.StoneID, stone);
    }
    
    public void UnregisterStone(TurnableStone stone)
    {
        if (stone == null) return;

        if (stoneLookup.ContainsKey(stone.StoneID))
            stoneLookup.Remove(stone.StoneID);
    }
    
    private void CheckAllTriggers()//CheckTriggers
    {
        foreach (StoneEventTrigger trigger in eventTriggers)
        {
            if (trigger.hasBeenTriggered || trigger.stoneConfig == null)
                continue;
            
            if (IsPuzzleSolved(trigger.stoneConfig))
            {
                trigger.hasBeenTriggered = true;
                trigger.onPuzzleSolved.Invoke();
                
                Debug.Log($"Puzzle Solved: {trigger.triggerName}");
            }
        }
    }
    
    private bool IsPuzzleSolved(StoneConfiguration config)
    {
        Debug.Log($"Checking config: {config.name}");
        foreach (var requirement in config.requiredStones)
        {
            Debug.Log($"Checking requirement: {requirement.stoneID}");
            if (!requirement.IsSatisfied(stoneLookup))
                return false;
        }
        Debug.Log("PUZZLE SOLVED TRIGGER FIRED");
        return true;
    }
    
    public void AddEventTrigger(string name, StoneConfiguration config)
    {
        var trigger = new StoneEventTrigger
        {
            triggerName = name,
            stoneConfig = config
        };
        eventTriggers.Add(trigger);
    }
    
    public void ResetAllTriggers()
    {
        foreach (var trigger in eventTriggers)
            trigger.hasBeenTriggered = false;
    }
    
    #if UNITY_EDITOR
    public List<StoneEventTrigger> GetEventTriggers() => eventTriggers;
    #endif
}
