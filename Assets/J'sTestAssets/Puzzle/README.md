# J'sTestAssets Organization

## Folder Structure

```
J'sTestAssets/
├── Puzzle/                          # All puzzle and event system code
│   ├── Framework/                   # Core event system (do not modify)
│   │   ├── GameEvents.cs            # Central event dispatcher
│   │   ├── IActivatorRequirement.cs # Interface for requirements
│   │   ├── ActivatorState.cs        # Generic state container
│   │   ├── StateEvent.cs
│   │   ├── StateListener.cs
│   │   └── SignalListener.cs
│   │
│   ├── Configurations/              # ScriptableObject configs (define puzzle rules)
│   │   ├── ActivatorConfiguration.cs    # Abstract base class
│   │   ├── StoneConfiguration.cs        # Rotating stone puzzle rules
│   │   ├── LeverConfiguration.cs        # Lever puzzle rules
│   │   └── Assets/                      # .asset files (created in Editor)
│   │       ├── StoneConfigArea1.asset
│   │       └── LeverConfig.asset
│   │
│   ├── Activators/                  # Runtime behavior scripts
│   │   ├── StoneActivator.cs        # Stone behavior & event emission
│   │   ├── LeverActivator.cs        # Lever behavior & event emission
│   │
│   └── Validators/                  # Puzzle validation logic
│       └── PuzzleValidator.cs       # Checks if puzzles are solved
│
├── Gameplay/                        # Other gameplay mechanics
│   └── MovingPlatform.cs
│
├── Art/                             # Visual assets
│   ├── Materials/
│   ├── Mesh/
│   ├── Port/
│   ├── TextMesh Pro/
│   └── ... (other art assets)
│
├── Scripts/                         # Existing scripts folder (legacy)
│
└── Prefabs/                         # Prefabs (rename from Prefabs when applicable)
    ├── AstronautTest.prefab
    ├── EnemyTest.prefab
    ├── Enemy_LittleGreenAlien.prefab
    └── ... (other prefabs)
```

## How to Add a New Activator Type

1. **Create a Configuration** in `Puzzle/Configurations/`:
   ```csharp
   public class MyActivatorConfiguration : ActivatorConfiguration
   {
       public class MyRequirement : IActivatorRequirement { ... }
       public override IActivatorRequirement[] GetRequirements() { ... }
   }
   ```

2. **Create an Activator** in `Puzzle/Activators/`:
   ```csharp
   public class MyActivator : MonoBehaviour
   {
       public void SetState(object state)
       {
           GameEvents.RaiseActivatorStateChanged(id, state);
       }
   }
   ```

3. **Create a ScriptableObject** in `Puzzle/Configurations/Assets/`
   - Use the Editor menu: `Create > Puzzle > My Activator Configuration`
   - Assign to a `PuzzleValidator` trigger

## Event Flow

1. **Activator** changes state (e.g., stone rotates, lever pulls)
2. **Activator** calls `GameEvents.RaiseActivatorStateChanged(id, state)`
3. **PuzzleValidator** receives event, updates its state cache
4. **PuzzleValidator** checks all puzzle triggers
5. If all requirements satisfied, **UnityEvent** fires (e.g., door opens)

## Key Files

- **GameEvents.cs** - Central event hub (modify to add new event types)
- **IActivatorRequirement.cs** - Interface all requirements must implement
- **PuzzleValidator.cs** - Main puzzle checking logic (should not need modification)
