# Quick Reference

## System Overview

```
Activator emits state → PuzzleValidator receives → Checks Configuration → Fires event
```

## Files You Need

1. **Configuration** (`Puzzle/Configurations/YourConfig.cs`) - Defines puzzle rules
2. **Activator**(With activation logic) (`Puzzle/Activators/YourActivator.cs`) - Sends state changes
3. **Asset** (Created in Editor) - Assigns Configuration to puzzle

---

## Setup Steps

1. Create Configuration → extends `ActivatorConfiguration`
2. Create Activator → calls `GameEvents.RaiseActivatorStateChanged()`
3. In Editor: Right-click `Puzzle/Configurations/Assets/` → Create → Puzzle → Your Type
4. Assign asset to PuzzleValidator trigger
5. Attach Activator script to GameObject

---

## How to Add a New Activator Type

1. **Step 1: Create a Configuration** in `Puzzle/Configurations/`:

   ```csharp
   public class MyActivatorConfiguration : ActivatorConfiguration
   {
       public class MyRequirement : IActivatorRequirement { ... }
       public override IActivatorRequirement[] GetRequirements() { ... }
   }
   ```

2. **Step 2: Create an Activator** in `Puzzle/Activators/`:

   ```csharp
   public class MyActivator : MonoBehaviour
   {
       public void SetState(object state)
       {
           GameEvents.RaiseActivatorStateChanged(id, state);
       }
   }
   ```

3. **Step 3-5: Create a ScriptableObject**
    1. Right-click in `Puzzle/Configurations/Assets/` → Create → Puzzle → Your Type
    2. Assign to PuzzleValidator trigger
    3. Attach YourActivator to GameObject

---

## Defined State Types

| Type | Send | Check |
|------|------|-------|
| `bool` | `GameEvents.RaiseActivatorStateChanged(id, true);` | `state is bool b` |
| `float` | `GameEvents.RaiseActivatorStateChanged(id, 45f);` | `state is float f` |
| `int` | `GameEvents.RaiseActivatorStateChanged(id, 3);` | `state is int i` |

---


## Troubleshooting

| Problem | Check |
|---------|-------|
| Puzzle not solving | IDs match? Asset assigned? |
| Events not received | Call `GameEvents.RaiseActivatorStateChanged()`? |
| Wrong requirements | State type matches? |

---
## Templates

### Configuration

```csharp
[CreateAssetMenu(fileName = "YourConfig", menuName = "Puzzle/Your Type")]
public class YourConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class YourRequirement : IActivatorRequirement
    {
        [SerializeField] private string activatorID = "id_1";
        public string ActivatorID => activatorID;
        [SerializeField] private bool mustBeActive = true;
        
        public bool IsSatisfied(object state)
        {
            return state is bool b && b == mustBeActive;
        }
    }
    
    [SerializeField] public YourRequirement[] required;
    public override IActivatorRequirement[] GetRequirements() => required;
}
```

### Activator
```csharp
public class YourActivator : MonoBehaviour
{
    [SerializeField] private string activatorID = "id_1";
    private bool state = false;
    
    public void SetState(bool newState)
    {
        if (state == newState) return;
        state = newState;
        GameEvents.RaiseActivatorStateChanged(activatorID, state);
    }
}
```

---
