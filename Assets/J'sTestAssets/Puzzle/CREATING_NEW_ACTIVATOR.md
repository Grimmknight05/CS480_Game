# Creating a New Activator

## System Overview

```
Activator emits state → PuzzleValidator receives → Checks Configuration → Fires event
```

## Files You Need

1. **Configuration** (`Puzzle/Configurations/YourConfig.cs`) - Defines puzzle rules
2. **Activator**(With activation logic) (`Puzzle/Activators/YourActivator.cs`) - Sends state changes
3. **Asset** (Created in Editor) - Assigns Configuration to puzzle

---

## Step 1: Configuration Class

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
            return state is bool active && active == mustBeActive;
        }
    }

    [SerializeField] public YourRequirement[] required;
    public override IActivatorRequirement[] GetRequirements() => required;
}
```

---

## Step 2: Activator Script

```csharp
public class YourActivator : MonoBehaviour
{
    [SerializeField] private string activatorID = "id_1";
    private bool state = false;

    public void SetState(bool newState)
    {
        if (state == newState) return;
        state = newState;
        GameEvents.RaiseActivatorStateChanged(activatorID, state); // KEY LINE
    }
}
```

---

## Step 3: Editor Setup

1. Right-click in `Puzzle/Configurations/Assets/` → Create → Puzzle → Your Type
2. Assign to PuzzleValidator trigger
3. Attach YourActivator to GameObject

---

## State Types

| Type | Example | Check |
|------|---------|-------|
| `bool` | Pressed/released | `state is bool b` |
| `float` | Rotation angle | `state is float f` |
| `int` | Press count | `state is int i` |

---