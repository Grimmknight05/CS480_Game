# Puzzle System Overview

The Puzzle System validates whether a set of in-world **activators** (levers, pressure plates, turnable stones, mushrooms, …) match the rules defined by a **configuration** asset, and fires `UnityEvent`s when they do or stop matching. It is the project's general-purpose "is this puzzle solved?" checker.

The architecture is decoupled in three layers:

1. **Activators** (MonoBehaviours) — detect input or world state and broadcast typed events through a ScriptableObject channel.
2. **Typed `ActivatorStateChannel<T>`** (ScriptableObject) — a strongly-typed shared bus every activator publishes on and the validator subscribes to.
3. **`PuzzleValidator`** (MonoBehaviour, implements `IPuzzleStateProvider`) — listens to one or more channels, stores typed state snapshots, and calls `config.IsSolved(this)` on each trigger whenever state changes.

> **This document supersedes** `Assets/Systems/Puzzle/README.md`, `CREATING_NEW_ACTIVATOR.md`, and `QUICK_REFERENCE.md`. Those files describe the legacy `GameEvents`-static and untyped-`object` paths that have been removed.

---

## Core Components

### Framework (the contract)

All types live under `Assets/Systems/Puzzle/Framework/`.

- **[`ActivatorID`](../Assets/Systems/Puzzle/Framework/ActivatorID.cs)** — ScriptableObject used as a typo-proof puzzle identifier. Replaces magic strings entirely. Both the activator component in the scene and the configuration asset hold a reference to the **same** `ActivatorID` asset — the Editor enforces the join instead of relying on exact string matches.
  - *Create via*: right-click → **Create → Puzzle → Activator ID**.

- **[`IActivatorRequirement<T>`](../Assets/Systems/Puzzle/Framework/IActivatorRequirement.cs)** — generic interface every requirement implements:
  - `ActivatorID ActivatorID { get; }` — which activator this requirement is asking about.
  - `bool IsSatisfied(T state)` — given the strongly-typed current state for that ID, return whether this requirement is met.

- **[`IPuzzleStateProvider`](../Assets/Systems/Puzzle/Framework/IPuzzleStateProvider.cs)** — interface that decouples `ActivatorConfiguration` from the concrete validator. Any class that holds typed state can implement it and pass itself to `IsSolved`. Current methods:
  ```csharp
  bool TryGetBool(ActivatorID id, out bool value);
  bool TryGetFloat(ActivatorID id, out float value);
  bool TryGetMushroomColor(ActivatorID id, out MushroomColor value);
  bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value);
  ```

- **[`ActivatorStateChannel<T>`](../Assets/Systems/Puzzle/Framework/ActivatorStateChannel.cs)** — abstract generic SO base:
  - `event Action<ActivatorID, T> OnStateChanged`
  - `RaiseEvent(ActivatorID id, T state)` — null-guards the ID and invokes subscribers.
  - `OnDisable()` — nulls `OnStateChanged` to prevent ghost listeners across domain reloads.

### Typed Channels (concrete SO assets)

| Class | Menu path | Payload | Use |
|---|---|---|---|
| `BoolActivatorChannel` | Events → Bool Activator Channel | `bool` | Levers, pressure plates |
| `FloatActivatorChannel` | Events → Float Activator Channel | `float` | Turnable stones, boss pillars |
| `MushroomColorChannel` | Events → Mushroom Color Channel | `MushroomColor` | Single-mushroom color events |
| `MushroomColorArrayChannel` | Events → Mushroom Color Array Channel | `MushroomColor[]` | Musical sequence tracker → validator |

Create one asset per puzzle that needs that type. A single channel asset can be shared across multiple puzzles in the same scene — `ActivatorID` disambiguates which activator sent the event.

### Configuration (the rules — `ScriptableObject`)

All configuration types live under `Assets/Systems/Puzzle/Configurations/`.

- **[`ActivatorConfiguration`](../Assets/Systems/Puzzle/Configurations/ActivatorConfiguration.cs)** — abstract base. Single abstract member:
  ```csharp
  public abstract bool IsSolved(IPuzzleStateProvider state);
  ```
  Concrete subclasses query the provider via `TryGetBool` / `TryGetFloat` / etc. and iterate their own requirements internally. There is no public `GetRequirements()` — the validator never inspects requirements directly.

- **[`StoneConfiguration`](../Assets/Systems/Puzzle/Configurations/StoneConfiguration.cs)** — `StoneRequirement : IActivatorRequirement<float>`. Fields: `stoneID (ActivatorID)`, `activationRotation`, `rotationTolerance`, `requireSpecificRotation`. State queried via `TryGetFloat`. Match: `Mathf.Abs(Mathf.DeltaAngle(rotation, activationRotation)) <= rotationTolerance`.

- **[`PressurePlateConfiguration`](../Assets/Systems/Puzzle/Configurations/PressurePlateConfiguration.cs)** — `PressurePlateRequirement : IActivatorRequirement<bool>`. Fields: `plateID (ActivatorID)`, `mustBePressed`. State queried via `TryGetBool`.

- **[`LeverConfiguration`](../Assets/Systems/Puzzle/Configurations/LeverConfiguration.cs)** — `LeverRequirement : IActivatorRequirement<bool>`. Fields: `leverID (ActivatorID)`, `mustBeEngaged`. State queried via `TryGetBool`.

- **[`MushroomConfiguration`](../Assets/Systems/Puzzle/Configurations/MushroomConfiguration.cs)** — `MushroomRequirement : IActivatorRequirement<MushroomColor>`. Fields: `mushroomID (ActivatorID)`, `requireSpecificColor`, `expectedColor`. State queried via `TryGetMushroomColor`.

- **[`MusicalSequenceConfiguration`](../Assets/Systems/MusicalPuzzle/Validation/MusicalSequenceConfiguration.cs)** — `SequenceRequirement : IActivatorRequirement<MushroomColor[]>`. Fields: `sequenceID (ActivatorID)`, `expectedSequence[]`, `logComparisonChecks`. State queried via `TryGetMushroomColorArray`. Matches a sliding-window suffix of the live history against the expected sequence. Also exposes `ExpectedSequence` for the `MushroomSequenceTracker` to drive wrong-note detection.

### Validator (the consumer)

- **[`PuzzleValidator`](../Assets/Systems/TestingSOChannels/PuzzleValidator.cs)** — MonoBehaviour implementing `IPuzzleStateProvider`. Holds:
  - Up to four typed channel slots: `boolChannel`, `floatChannel`, `mushroomChannel`, `mushroomArrayChannel` — assign only the ones used by your puzzles.
  - `List<PuzzleTrigger> triggers` — each entry pairs an `ActivatorConfiguration` with `UnityEvent onSolved` / `onUnsolved` and `bool reTriggerable`.
  - Four internal typed dictionaries (`Dictionary<ActivatorID, bool/float/MushroomColor/MushroomColor[]>`) — the running state snapshot.
  - Subscribes to all assigned channels in `OnEnable`, unsubscribes in `OnDisable` (ghost-listener safe).
  - On every state change, calls `trigger.config.IsSolved(this)` for each trigger, passing itself as the `IPuzzleStateProvider`.

### Active Activators

- **[`LeverActivator`](../Assets/Systems/Puzzle/Activators/LeverActivator.cs)** — bool publisher. Fields: `stateChannel (BoolActivatorChannel)`, `leverID (ActivatorID)`. Broadcasts initial state in `Start()` so the validator is seeded before the player touches anything. Calls `stateChannel.RaiseEvent(leverID, isEngaged)` on `SetEngaged(bool)`.

- **[`PressurePlateIntegrated`](../Assets/Systems/Puzzle/Activators/PressurePlateIntegrated.cs)** — bool publisher. Fields: `stateChannel (BoolActivatorChannel)`, `plateID (ActivatorID)`. Tracks colliders by tag in a `HashSet<Collider>`; raises `true` on first entry, `false` when empty. Supports a `visual` Transform that drops `pressedDrop` units on press.

- **[`TurnableStone`](../Assets/Systems/TestingSOChannels/TurnableStone.cs)** — float publisher. Fields: `stateChannel (FloatActivatorChannel)`, `stoneID (ActivatorID)`. Raises `stateChannel.RaiseEvent(stoneID, rotation)` on rotation change.

- **[`Mushroom`](../Assets/Systems/MusicalPuzzle/Mushroom/Mushroom.cs)** — single-color publisher (via state machine). Fields: `puzzleChannel (MushroomColorChannel)`, `puzzleActivatorID (ActivatorID)`. `ActiveState.Enter` raises `puzzleChannel.RaiseEvent(puzzleActivatorID, assignedColor)`. The `mushroomChannel (MushroomEventChannelSO)` is a separate, richer broadcast used by critters and audio — not the puzzle channel.

- **[`MushroomSequenceTracker`](../Assets/Systems/MusicalPuzzle/Validation/MushroomSequenceTracker.cs)** — sequence publisher. Fields: `puzzleChannel (MushroomColorArrayChannel)`, `melodyConfiguration (MusicalSequenceConfiguration)`. Accumulates a bounded `List<MushroomColor>` of activations, applies wrong-note prefix detection, and re-raises the running snapshot via `puzzleChannel.RaiseEvent(melodyConfiguration.SequenceActivatorID, snapshot)`. The sequence ID SO comes from the config asset — same asset wired into both tracker and validator is the single source of truth.

---

## The Event Flow

End-to-end for a float puzzle (turnable stones):

1. **Activator publishes.** `TurnableStone` calls `stateChannel.RaiseEvent(stoneID, rotation)` — `(ActivatorID, float)`.
2. **Channel broadcasts.** `FloatActivatorChannel.RaiseEvent` invokes `OnStateChanged?.Invoke(id, state)`.
3. **Validator records.** `PuzzleValidator.HandleFloatChanged` writes `floatStates[id] = value`, then calls `CheckAllPuzzles()`.
4. **For each `PuzzleTrigger`:** calls `config.IsSolved(this)` — the config queries the validator (as `IPuzzleStateProvider`) via `TryGetFloat(requirement.ActivatorID, out float value)` for each of its requirements.
5. **Edge-triggered fire.** `PuzzleTrigger` tracks `isCurrentlySolved` (`[NonSerialized]` — resets cleanly on domain reload). On unsolved → solved: `onSolved.Invoke()`. On solved → unsolved: `onUnsolved.Invoke()`. If `reTriggerable` is `false`, `onSolved` only fires once.

The flow is identical for bool puzzles (levers/plates) via `BoolActivatorChannel` + `TryGetBool`, and for mushroom puzzles via the respective mushroom channels.

---

## Editor Setup & Wiring

### One-time per-project assets

Create these shared assets once; reuse them across scenes.

1. **Typed channel assets.** Right-click in `Assets/Systems/Events/` → **Create → Events → [Bool / Float / Mushroom Color / Mushroom Color Array] Activator Channel**. Name clearly, e.g., `BoolPuzzleChannel.asset`, `FloatPuzzleChannel.asset`.

2. **`ActivatorID` assets.** Right-click in a logical folder (e.g., `Assets/Systems/Puzzle/IDs/Area1/`) → **Create → Puzzle → Activator ID**. Create one per physical activator in the world. Name it to match the object, e.g., `Stone_Area1_Left.asset`. These are the shared references that tie a scene component to a config asset.

### Per-puzzle setup

3. **Create the configuration asset.** Right-click in `Assets/Systems/Puzzle/Configurations/Assets/[AreaN]/`:
   - **Create → Puzzle → Stone Configuration** — for stone rotation puzzles.
   - **Create → Puzzle → Pressure Plate Configuration** — for plate puzzles.
   - **Create → Puzzle → Lever Configuration** — for lever puzzles.
   - **Create → Puzzle → Mushroom Configuration** — for single-color mushroom checks.
   - **Create → Puzzle → Musical Sequence Configuration** — for sequence melody puzzles.

4. **Fill in the requirements.** Each configuration exposes a requirements array. For each entry:
   - Drag the relevant **`ActivatorID` SO** into the ID slot (not a string — the field is now typed).
   - Set the target state: `activationRotation` + `rotationTolerance` for stones, `mustBePressed` for plates, `mustBeEngaged` for levers, `expectedColor` for mushrooms, `expectedSequence[]` for sequences.

### Per-scene wiring

5. **Validator GameObject.** Place an empty GameObject (e.g., `_PuzzleValidator`) in the scene. Attach `PuzzleValidator`.
   - Drag the appropriate typed channel assets into the relevant channel slots (**Bool Channel**, **Float Channel**, etc.). Only assign slots that your puzzles actually use.
   - Add a **Trigger** entry for each puzzle:
     - `triggerName` — for Inspector readability.
     - `config` — drag the configuration asset.
     - `onSolved` / `onUnsolved` — wire `UnityEvent` actions.
     - `reTriggerable` — off for one-shot rewards; on for reset-able puzzles.

6. **Activator GameObjects.** For each activator, assign the **same `ActivatorID` SO** that the configuration uses, and assign the **same typed channel** that the validator listens to:

   | Activator | Channel field type | ID field type |
   |---|---|---|
   | `LeverActivator` | `BoolActivatorChannel` | `ActivatorID` |
   | `PressurePlateIntegrated` | `BoolActivatorChannel` | `ActivatorID` |
   | `TurnableStone` | `FloatActivatorChannel` | `ActivatorID` |
   | `Mushroom` | `MushroomColorChannel` | `ActivatorID` (puzzleActivatorID) |
   | `MushroomSequenceTracker` | `MushroomColorArrayChannel` | *(comes from MusicalSequenceConfiguration)* |

### Inspector field summary

| Component | Field | Purpose |
|---|---|---|
| `PuzzleValidator` | `boolChannel` | Subscribe to bool activator events (levers, plates). |
| `PuzzleValidator` | `floatChannel` | Subscribe to float activator events (stones). |
| `PuzzleValidator` | `mushroomChannel` | Subscribe to single-color mushroom events. |
| `PuzzleValidator` | `mushroomArrayChannel` | Subscribe to sequence snapshot events. |
| `PuzzleValidator` | `triggers` | One entry per puzzle this validator checks. |
| `LeverActivator` | `stateChannel (BoolActivatorChannel)`, `leverID (ActivatorID)` | Channel + ID. |
| `PressurePlateIntegrated` | `stateChannel (BoolActivatorChannel)`, `plateID (ActivatorID)`, `acceptedTags`, `visual`, `pressedDrop` | Channel + ID + optional visual feedback. |
| `TurnableStone` | `stateChannel (FloatActivatorChannel)`, `stoneID (ActivatorID)` | Channel + ID. |
| `Mushroom` | `puzzleChannel (MushroomColorChannel)`, `puzzleActivatorID (ActivatorID)` | Channel + ID for puzzle events. |
| `MushroomSequenceTracker` | `puzzleChannel (MushroomColorArrayChannel)`, `melodyConfiguration` | Channel + config (config provides the ActivatorID). |
| `StoneConfiguration` | `requiredStones[]` — each: `stoneID (ActivatorID)`, `activationRotation`, `rotationTolerance` | The rules. |
| `PressurePlateConfiguration` | `requiredPlates[]` — each: `plateID (ActivatorID)`, `mustBePressed` | The rules. |
| `LeverConfiguration` | `requiredLevers[]` — each: `leverID (ActivatorID)`, `mustBeEngaged` | The rules. |
| `MushroomConfiguration` | `required[]` — each: `mushroomID (ActivatorID)`, `requireSpecificColor`, `expectedColor` | The rules. |
| `MusicalSequenceConfiguration` | `requirement` — `sequenceID (ActivatorID)`, `expectedSequence[]` | The rules + shared sequence ID. |

---

## Code Examples / Templates

### Template: a new configuration type

Extend `ActivatorConfiguration`. The nested `Requirement` class implements `IActivatorRequirement<T>` for whatever state type your activator publishes. `IsSolved` queries the provider and iterates requirements.

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "TimedSwitchConfig", menuName = "Puzzle/Timed Switch Configuration")]
public class TimedSwitchConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class TimedSwitchRequirement : IActivatorRequirement<float>
    {
        [SerializeField] private ActivatorID switchID;
        [SerializeField] private float minHeldSeconds = 1.5f;

        public ActivatorID ActivatorID => switchID;
        public bool IsSatisfied(float heldSeconds) => heldSeconds >= minHeldSeconds;
    }

    [SerializeField] private TimedSwitchRequirement[] requiredSwitches;

    public override bool IsSolved(IPuzzleStateProvider state)
    {
        if (requiredSwitches == null || requiredSwitches.Length == 0) return false;
        foreach (var r in requiredSwitches)
        {
            if (r.ActivatorID == null) return false;
            if (!state.TryGetFloat(r.ActivatorID, out float value)) return false;
            if (!r.IsSatisfied(value)) return false;
        }
        return true;
    }
}
```

If your state type is not `bool`, `float`, `MushroomColor`, or `MushroomColor[]`, you must also:
1. Create a new `ActivatorStateChannel<YourType>` concrete subclass.
2. Add `bool TryGetYourType(ActivatorID id, out YourType value)` to `IPuzzleStateProvider`.
3. Implement the new method in `PuzzleValidator` with a matching dictionary and channel.

### Template: a new activator

```csharp
using UnityEngine;

public class TimedSwitchActivator : MonoBehaviour
{
    [SerializeField] private FloatActivatorChannel stateChannel; // match type to config
    [SerializeField] private ActivatorID switchID;               // same SO as config requirement

    private float heldSeconds;
    private bool held;

    private void Start()
    {
        // Seed the validator with initial state so it doesn't wait for first interaction.
        Publish();
    }

    void Update()
    {
        if (held) { heldSeconds += Time.deltaTime; Publish(); }
    }

    public void OnPressStart() { held = true; heldSeconds = 0f; }
    public void OnPressEnd()   { held = false; Publish(); }

    private void Publish()
    {
        if (stateChannel == null || switchID == null) return;
        stateChannel.RaiseEvent(switchID, heldSeconds);
    }
}
```

The channel type the activator uses **must match** the type `IPuzzleStateProvider` method the configuration queries. This is now enforced at the channel level — a `FloatActivatorChannel` cannot accidentally deliver a `bool`.

### Template: extending `IPuzzleStateProvider` for a new payload type

When adding a new state type, update these three files in order:

**1. `IPuzzleStateProvider.cs`** — add the method:
```csharp
bool TryGetMyType(ActivatorID id, out MyType value);
```

**2. `PuzzleValidator.cs`** — add a channel field, a dictionary, a handler, and implement the method:
```csharp
[SerializeField] private MyTypeChannel myTypeChannel;
private readonly Dictionary<ActivatorID, MyType> myTypeStates = new();

// OnEnable: myTypeChannel.OnStateChanged += HandleMyTypeChanged;
// OnDisable: myTypeChannel.OnStateChanged -= HandleMyTypeChanged;

private void HandleMyTypeChanged(ActivatorID id, MyType value) { myTypeStates[id] = value; CheckAllPuzzles(); }
public bool TryGetMyType(ActivatorID id, out MyType value) => myTypeStates.TryGetValue(id, out value);
```

**3. Any other `IPuzzleStateProvider` implementors** (e.g., `WaveSpawnPhase`) — add a stub:
```csharp
public bool TryGetMyType(ActivatorID id, out MyType value) { value = default; return false; }
```

---

## Scene Rewiring Guide (post-refactor)

The `arch/puzzle-system-refinement` branch changed all activator ID fields from `string` to `ActivatorID` SO, and all channel fields to typed concrete channels. Existing scenes need the following work before puzzles function.

### Step 1 — Create `ActivatorID` assets

For every activator in the scene, create one `ActivatorID` SO and name it to match the old string value (e.g., old `stoneID = "stone_area1_1"` → new asset named `Stone_Area1_1`). Place them in a logical folder, e.g., `Assets/Systems/Puzzle/IDs/[AreaN]/`.

### Step 2 — Create typed channel assets

Create the channel SOs your scene needs (once per type, shared across puzzles):
- `BoolPuzzleChannel.asset` → `BoolActivatorChannel`
- `FloatPuzzleChannel.asset` → `FloatActivatorChannel`
- *(Mushroom channels if applicable)*

### Step 3 — Rewire configuration assets

Open each `.asset` config in the Inspector. The old `string` ID fields are now `ActivatorID` object slots — drag the matching SO into each slot. The numeric values (`activationRotation`, `rotationTolerance`, `mustBePressed`, etc.) are preserved.

### Step 4 — Rewire scene components

For each activator component in the scene:
- Drag the **same `ActivatorID` SO** used in the config into the component's ID field.
- Drag the **typed channel asset** into the channel field.

For each `PuzzleValidator`:
- Assign the typed channel assets to the relevant channel slots.
- Verify each trigger still has its config asset assigned (these references survive the refactor).

### Step 5 — Mushroom puzzles

For each `Mushroom` component: assign `puzzleChannel (MushroomColorChannel)` and `puzzleActivatorID (ActivatorID)`.

For each `MushroomSequenceTracker`: assign `puzzleChannel (MushroomColorArrayChannel)`. The sequence `ActivatorID` is read from the linked `MusicalSequenceConfiguration` — make sure that config's `sequenceID` slot has an SO assigned, and the same SO is referenced by the `PuzzleValidator`'s `mushroomArrayChannel`-backed trigger.

---

## Existing Configuration Assets

These assets exist in the project. Their string ID fields **need to be replaced** with `ActivatorID` SO references as part of the scene rewiring above.

```
Assets/Systems/Puzzle/Configurations/
├── StoneConfigIntro.asset
└── Assets/
    ├── LeverConfig.asset
    ├── Area1/  StoneConfigArea1.asset, StoneConfigArea1_1.asset, _2.asset, _3.asset
    ├── Area2/  Area 2 PressurePlates.asset
    ├── Area3/  Area3.asset, Area3_2.asset, _3.asset, _4.asset
    │           Area3_1/ Stone3Lock.asset, Stone4Lock.asset
    └── Area4/  Area4RotateOnTurn.asset, Area4_1Lock.asset, _2Lock.asset, _3Lock.asset
                Area4_1_Pillar.asset, _2_Pillar.asset, _3_Pillar.asset, _3_Second.asset

Assets/Systems/Puzzle/Activators/PressurePlateConfig1.asset
Assets/Systems/Puzzle/Configurations/Assets/Boss/  BossWave1.asset, BossWave2.asset, BossWave3.asset
Assets/Systems/MusicalPuzzle/Events/BasicMusicalSequence.asset
```

---

## Notes / Caveats

- **`ActivatorID` is the join key.** Both the activator in the scene and the configuration requirement must reference the **same SO asset instance**. Two different SO assets with the same `description` text are not equal — Unity compares by reference.
- **`PuzzleValidator` only knows the *latest* state per ID.** The dictionary is overwritten on every event. There is no history, debounce, or time window. Compute summary values in the activator before publishing.
- **`onSolved` does not auto-undo.** If you open a door on `onSolved`, explicitly close it on `onUnsolved`, or set `reTriggerable = false` for one-shot triggers.
- **`reTriggerable = false` is per-session.** `hasFired` and `isCurrentlySolved` are `[NonSerialized]` — they reset on domain reload / scene load, which is the correct behavior per the Editor Persistence Protocol (CLAUDE.md).
- **One channel can serve many puzzles.** `ActivatorID` disambiguates events. Multiple channels are useful when you want to scope activation events (e.g., a separate channel for boss-phase stones vs. world stones).
- **`WaveSpawnPhase` implements `IPuzzleStateProvider` directly.** It subscribes to `FloatActivatorChannel` (from `Boss.PhaseEntry.stateChannel`) and maintains its own float-state dictionary to check `StoneConfiguration`-based boss puzzles without going through the scene `PuzzleValidator`.
- **Adding a new state type** requires updating `IPuzzleStateProvider`, `PuzzleValidator`, and any other `IPuzzleStateProvider` implementors (`WaveSpawnPhase`). See the template above.
