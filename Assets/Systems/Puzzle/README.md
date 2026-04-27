# Puzzle System Documentation

## Folder Structure

```
Puzzle/
├── Framework/ # Event system
│   ├── GameEvents.cs
│   └── IActivatorRequirement.cs
├── Configurations/ # Puzzle rules (ScriptableObjects)
│   ├── *Configuration.cs
│   └── Assets/
│       └── *.asset
├── Activators/ # Runtime behavior
│   ├── TurnableStone.cs
│   ├── LeverActivator.cs
│   └── ...
└── Validators/ #Event delegator
    └── PuzzleValidator.cs
```

## How to Add a New Activator

1. Create Configuration class → extends `ActivatorConfiguration`
2. Create Activator script → calls `GameEvents.RaiseActivatorStateChanged()`
3. Create `.asset` in Editor
4. Assign to PuzzleValidator trigger

## Event Flow

Activator → emits state → PuzzleValidator → checks Configuration → fires UnityEvent

## Key Files

- **GameEvents.cs** - Central event hub (modify to add new event types)
- **IActivatorRequirement.cs** - Interface all requirements must implement
- **PuzzleValidator.cs** - Main puzzle checking logic (should not need modification)

---

## Real Examples in Project

| Activator | Configuration | State | File |
|-----------|---------------|-------|------|
| **Stone** | StoneConfiguration | `float` (angle) | `TurnableStone.cs` |
| **Lever** | LeverConfiguration | `bool` (pulled) | `LeverActivator.cs` |
| **Button** | ButtonConfiguration | `int` (count) | `ButtonActivator.cs` |

---
