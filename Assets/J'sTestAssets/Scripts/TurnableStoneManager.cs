using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TurnableStoneManager : MonoBehaviour
{
    public static TurnableStoneManager Instance { get; private set; }
    
    [System.Serializable]
    public class StoneEventTrigger
    {
        public string triggerName;
        public StoneConfiguration stoneConfig;
        public UnityEvent onPuzzleSolved = new UnityEvent();
        
        [HideInInspector] public bool hasBeenTriggered = false;
    }
    
    [SerializeField] private List<StoneEventTrigger> eventTriggers = new List<StoneEventTrigger>();
    [SerializeField] private float checkInterval = 0.2f; // Check every 0.2 seconds
    
    private List<TurnableStone> registeredStones = new List<TurnableStone>();
    private float checkTimer = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void RegisterStone(TurnableStone stone)
    {
        if (!registeredStones.Contains(stone))
            registeredStones.Add(stone);
    }
    
    public void UnregisterStone(TurnableStone stone)
    {
        registeredStones.Remove(stone);
    }
    
    private void Update()
    {
        checkTimer += Time.deltaTime;
        
        if (checkTimer >= checkInterval)
        {
            CheckAllTriggers();
            checkTimer = 0f;
        }
    }
    
    private void CheckAllTriggers()
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
        foreach (var requirement in config.requiredStones)
        {
            if (requirement.stone == null || !requirement.IsSatisfied())
                return false;
        }
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
