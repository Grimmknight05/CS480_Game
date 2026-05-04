# Puzzle System Overview

The Puzzle System validates whether a set of in-world **activators** (levers, pressure plates, turnable stones, …) match the rules defined by a **configuration** asset, and fires `UnityEvent`s when they do or stop matching. It is the project's general-purpose "is this puzzle solved?" checker.

The architecture is decoupled in three layers:

1. **Activators** (MonoBehaviours) — broadcast their state when something happens to them.
2. **`ActivatorStateChannel`** (ScriptableObject) — a shared bus every activator publishes on and the validator subscribes to.
3. **`PuzzleValidator`** (MonoBehaviour) — listens to the channel, checks each configured puzzle's `IActivatorRequirement[]`, fires `onSolved` / `onUnsolved` when state changes the answer.

> **This document supersedes** the older `Assets/Systems/Puzzle/README.md`, `CREATING_NEW_ACTIVATOR.md`, and `QUICK_REFERENCE.md`. Those files describe a `GameEvents`-based event path that **no longer drives `PuzzleValidator`** — see [Known Wiring Discrepancy](#known-wiring-discrepancy-leveractivator-is-orphaned) below.

## Core Components

### Framework (the contract)

- **[`IActivatorRequirement`](../Assets/Systems/Puzzle/Framework/IActivatorRequirement.cs)** — interface every requirement implements. Two members:
  - `string ActivatorID { get; }` — the ID this requirement is asking about.
  - `bool IsSatisfied(object activatorState)` — given the current state for that ID, return whether this requirement is met. State comes in as `object`; the implementation pattern-matches with `is` (e.g., `if (activatorState is float rotation)`).
- **[`ActivatorState`](../Assets/Systems/Puzzle/Framework/ActivatorState.cs)** — small value struct: `{ ActivatorID, State, Time }`. Used by the legacy `GameEvents` path; the active path passes `(string, object)` directly.

### Configuration (the rules — `ScriptableObject`)

- **[`ActivatorConfiguration`](../Assets/Systems/Puzzle/Configurations/ActivatorConfiguration.cs)** — abstract base. Single member: `abstract IActivatorRequirement[] GetRequirements()`. Each concrete subclass declares its own nested `Requirement` class implementing `IActivatorRequirement`, and an array field of those.
- **[`StoneConfiguration`](../Assets/Systems/Puzzle/Configurations/StoneConfiguration.cs)** — `StoneRequirement { stoneID, activationRotation, rotationTolerance }`. State expected: `float` rotation in degrees. Match: `Mathf.Abs(Mathf.DeltaAngle(rotation, activationRotation)) <= rotationTolerance`.
- **[`PressurePlateConfiguration`](../Assets/Systems/Puzzle/Configurations/PressurePlateConfiguration.cs)** — `PressurePlateRequirement { plateID, mustBePressed }`. State expected: `bool`.
- **[`LeverConfiguration`](../Assets/Systems/Puzzle/Configurations/LeverConfiguration.cs)** — `LeverRequirement { leverID, mustBeEngaged }`. State expected: `bool`.

### Event Channel (the bus — `ScriptableObject`)

- **[`ActivatorStateChannel`](../Assets/Systems/TestingSOChannels/ActivatorStateChannel.cs)** — a SO with `event Action<string, object> OnStateChanged` and a `RaiseEvent(string activatorID, object state)` method. Logs a warning if raised with no subscribers. This is **separate** from `VoidEventChannelSO` (covered in [`EventChannels.md`](EventChannels.md)) because activator events carry a payload (the state) — the void channel can't.
- **Created via**: right-click → **Create → Events → Activator State Channel** (default file name `NewActivatorStateChannel.asset`).

### Validator (the consumer)

- **[`PuzzleValidator`](../Assets/Systems/TestingSOChannels/PuzzleValidator.cs)** — MonoBehaviour. Holds:
  - `ActivatorStateChannel stateChannel` — the bus it subscribes to in `OnEnable`.
  - `List<PuzzleTrigger> triggers` — each entry pairs an `ActivatorConfiguration` with `UnityEvent onSolved` / `UnityEvent onUnsolved` and a `bool reTriggerable`.
  - Internal `Dictionary<string, object> activatorStates` — the running snapshot of every `(activatorID, state)` it has seen.
- On every state change it calls `CheckAllPuzzles()`, which iterates each trigger's requirements and asks `IsSatisfied`.

### Active Activators

- **[`PressurePlateIntegrated`](../Assets/Systems/Puzzle/Activators/PressurePlateIntegrated.cs)** — bool publisher. Tracks colliders entering its trigger volume; on first occupant calls `stateChannel.RaiseEvent(plateID, true)`, on last leaving calls `stateChannel.RaiseEvent(plateID, false)`. Supports a `visual` Transform that drops `pressedDrop` on press for visual feedback. Filters by `acceptedTags`.
- **[`TurnableStone`](../Assets/Systems/TestingSOChannels/TurnableStone.cs)** — float publisher. Calls `stateChannel.RaiseEvent(stoneID, normalizedRotation)` on rotation change. Lives outside the `Puzzle/Activators/` folder but is logically a puzzle activator.
- **[`LeverActivator`](../Assets/Systems/Puzzle/Activators/LeverActivator.cs)** — bool publisher, **but currently calls the legacy `GameEvents` path** which no validator listens to. See below.
- **[`EnemyZoneActivator`](../Assets/Systems/Puzzle/Activators/EnemyZoneActivator.cs)** — empty stub.

### Legacy (still compiles, no subscribers)

- **[`GameEvents`](../Assets/Systems/Puzzle/Framework/Event/GameEvents.cs)** — static class with `OnActivatorStateChanged` and `OnStoneRotationChanged` events plus matching `Raise…` methods. **Nothing in the codebase subscribes to either event.** Retained for a transition that didn't fully migrate (see Known Issues).

## The Event Flow

The current, working path through the system, end to end:

1. **Activator publishes.** A `PressurePlateIntegrated` or `TurnableStone` calls `stateChannel.RaiseEvent(activatorID, state)` — `(string, bool)` for plates, `(string, float)` for stones.
2. **Channel broadcasts.** `ActivatorStateChannel.RaiseEvent` invokes `OnStateChanged?.Invoke(activatorID, state)`. If no subscribers, logs a warning.
3. **Validator records and re-checks.** `PuzzleValidator.HandleActivatorStateChanged` writes `activatorStates[activatorID] = state`, then calls `CheckAllPuzzles()`.
4. **For each `PuzzleTrigger`:**
   1. Get its `IActivatorRequirement[]` via `config.GetRequirements()`.
   2. For each requirement, look up `activatorStates[requirement.ActivatorID]`. If absent, the puzzle is not solved.
   3. If present, call `requirement.IsSatisfied(state)`. If any return false, the puzzle is not solved.
   4. If all return true, the puzzle is solved.
5. **Edge-triggered fire.** `PuzzleTrigger` tracks `isCurrentlySolved` and `hasFired`. On the transition unsolved → solved, `onSolved.Invoke()` runs; on solved → unsolved, `onUnsolved.Invoke()` runs. If `reTriggerable` is false, `onSolved` only ever fires once (`hasFired` blocks repeats).

### Known Wiring Discrepancy: `LeverActivator` is Orphaned

`LeverActivator.SetEngaged(bool)` calls `GameEvents.RaiseActivatorStateChanged(leverID, isEngaged)`. `PuzzleValidator` does **not** subscribe to `GameEvents.OnActivatorStateChanged` — it only listens to `ActivatorStateChannel.OnStateChanged`. **No other subscriber to `GameEvents.OnActivatorStateChanged` exists in the project** (verified by grep).

Practical effect: a `LeverConfiguration`-based puzzle today cannot be solved because the lever's state never reaches the validator. To fix this, `LeverActivator` should be migrated to use `ActivatorStateChannel` — see the [Migration template](#template-migrating-an-activator-from-gameevents-to-activatorstatechannel) below. Until then, treat `LeverConfiguration` as wired but inert.

## Editor Setup & Wiring

### One-time per-project setup

1. Create the shared event channel asset. In the **Project** window, right-click → **Create → Events → Activator State Channel**. Name it something like `PuzzleStateChannel.asset`. Place under `Assets/Systems/Events/` (an existing `StonePuzzleChannel.asset` lives there — you can reuse it or add a new one).

A single channel asset can be reused across many puzzles in many scenes. Each puzzle is identified by which **configuration asset** is plugged into the validator, not by which channel it uses.

### Per-puzzle setup

2. **Create the configuration asset.** Right-click in `Assets/Systems/Puzzle/Configurations/Assets/` (or a subfolder per area):
   - **Create → Puzzle → Stone Configuration** for stone puzzles.
   - **Create → Puzzle → Pressure Plate Configuration** for plate puzzles.
   - **Create → Puzzle → Lever Configuration** for lever puzzles. *(see the LeverActivator caveat above)*
3. **Fill in the requirements.** Each configuration exposes an array (`requiredStones`, `requiredPlates`, `requiredLevers`). For each entry, set the activator's ID and its target state:
   - `StoneRequirement`: `stoneID`, `activationRotation` (degrees), `rotationTolerance` (degrees of leniency).
   - `PressurePlateRequirement`: `plateID`, `mustBePressed`.
   - `LeverRequirement`: `leverID`, `mustBeEngaged`.

### Per-scene wiring

4. **Validator GameObject.** Place an empty GameObject (e.g., `_PuzzleValidator`) in the scene. Attach `PuzzleValidator`.
   - Drag the channel asset into **State Channel**.
   - Add an entry to **Triggers** for each puzzle this validator should check:
     - `triggerName` — for your own readability in the Inspector.
     - `config` — drag the configuration asset.
     - `onSolved` — wire up `UnityEvent` actions (open door, play sound, raise another channel, etc.).
     - `onUnsolved` — actions to run if the puzzle becomes unsolved (closes door, etc.). Leave empty if not needed.
     - `reTriggerable` — leave **off** for one-shot rewards; turn **on** for puzzles that should re-fire whenever they re-solve.

A single `PuzzleValidator` can host any number of triggers; many puzzles in one scene can share one validator and one channel.

5. **Activator GameObjects.** Place each activator and set its ID to match the requirements:
   - **Pressure plate**: add a trigger collider, attach `PressurePlateIntegrated`, drag the same channel into **State Channel**, set **Plate ID** to match the configuration's `plateID`. Optionally assign a child Transform to **Visual** for the press animation. Add tags to **Accepted Tags** that should be able to press the plate (default `"Pushable"`).
   - **Turnable stone**: attach `TurnableStone` to the rotatable mesh, drag the channel into **State Channel**, set **Stone ID** to match the configuration's `stoneID`.
   - **Lever** *(currently inert — see Known Issues)*: attach `LeverActivator`, set **Lever ID**.

### Inspector field summary

| Component | Field | Purpose |
|---|---|---|
| `PuzzleValidator` | `stateChannel` | Channel it subscribes to. Must match the channel activators publish on. |
| `PuzzleValidator` | `triggers` | One entry per puzzle. |
| `PuzzleValidator.PuzzleTrigger` | `triggerName`, `config`, `onSolved`, `onUnsolved`, `reTriggerable` | Per-puzzle wiring. |
| `PressurePlateIntegrated` | `stateChannel`, `plateID`, `acceptedTags`, `visual`, `pressedDrop` | What it publishes on, what tags trigger it, optional visual feedback. |
| `TurnableStone` | `stateChannel`, `stoneID` | Same channel as plates is fine — IDs disambiguate. |
| `StoneConfiguration` | `requiredStones[]` (each: `stoneID`, `activationRotation`, `rotationTolerance`) | The rules. |
| `PressurePlateConfiguration` | `requiredPlates[]` (each: `plateID`, `mustBePressed`) | The rules. |
| `LeverConfiguration` | `requiredLevers[]` (each: `leverID`, `mustBeEngaged`) | The rules. |

## Code Examples / Templates

### Template: a new configuration type

Mirror the existing three. The shape is always: outer `ActivatorConfiguration` subclass + nested `Requirement` class implementing `IActivatorRequirement`.

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "TimedSwitchConfig", menuName = "Puzzle/Timed Switch Configuration")]
public class TimedSwitchConfiguration : ActivatorConfiguration
{
    [System.Serializable]
    public class TimedSwitchRequirement : IActivatorRequirement
    {
        public string switchID;
        public float minHeldSeconds = 1.5f;

        public string ActivatorID => switchID;

        public bool IsSatisfied(object activatorState)
        {
            return activatorState is float heldSeconds && heldSeconds >= minHeldSeconds;
        }
    }

    [SerializeField] public TimedSwitchRequirement[] requiredSwitches;

    public override IActivatorRequirement[] GetRequirements() => requiredSwitches;
}
```

`IsSatisfied` runs against `object`, so always pattern-match (`is float`, `is bool`, `is int`) before reading the state. The validator passes whatever the activator published.

### Template: a new activator (the right way)

Use `ActivatorStateChannel`. Don't use `GameEvents` — it's not wired up.

```csharp
using UnityEngine;

public class TimedSwitchActivator : MonoBehaviour
{
    [SerializeField] private ActivatorStateChannel stateChannel;
    [SerializeField] private string switchID = "switch_1";

    private float heldSeconds;
    private bool held;

    void Update()
    {
        if (held) heldSeconds += Time.deltaTime;
    }

    public void OnPressStart()
    {
        held = true;
        heldSeconds = 0f;
    }

    public void OnPressEnd()
    {
        held = false;
        if (stateChannel != null) stateChannel.RaiseEvent(switchID, heldSeconds);
    }
}
```

Notes: the type the activator publishes (`float` here) must match what the configuration's `IsSatisfied` pattern-matches on. There is no compile-time check tying the two together — keep them in sync deliberately.

### Template: migrating an activator from `GameEvents` to `ActivatorStateChannel`

This is exactly the change `LeverActivator` needs. Before:

```csharp
public class LeverActivator : MonoBehaviour
{
    [SerializeField] private string leverID = "lever_1";
    private bool isEngaged;

    public void SetEngaged(bool engaged)
    {
        isEngaged = engaged;
        GameEvents.RaiseActivatorStateChanged(leverID, isEngaged); // dead path
    }
}
```

After:

```csharp
public class LeverActivator : MonoBehaviour
{
    [SerializeField] private ActivatorStateChannel stateChannel;
    [SerializeField] private string leverID = "lever_1";
    private bool isEngaged;

    public void SetEngaged(bool engaged)
    {
        isEngaged = engaged;
        if (stateChannel != null) stateChannel.RaiseEvent(leverID, isEngaged);
    }
}
```

Then, on every existing `LeverActivator` in the scene, drag the same channel asset that the validator uses into the new **State Channel** slot.

## Existing Configuration Assets

For reference — these are the configuration `.asset` files that already exist in the project. New assets follow the same `Create → Puzzle → …` pattern.

```
Assets/Systems/Puzzle/Configurations/
├── StoneConfigIntro.asset
└── Assets/
    ├── LeverConfig.asset
    ├── Area1/  StoneConfigArea1[_1.._3].asset
    ├── Area2/  Area 2 PressurePlates.asset
    └── Area3/  Area3[_1.._4].asset, Area3_1/Stone3Lock.asset, Stone4Lock.asset

Assets/Systems/Puzzle/Activators/PressurePlateConfig1.asset
Assets/Scripts/Interaction/Puzzle1Config.asset
Assets/Systems/Events/StonePuzzleChannel.asset
```

## Notes / Caveats

- **State is `object`-typed.** Each `IsSatisfied` implementation is responsible for type-checking with `is`. There is no type safety between an activator's published state and a configuration's expected type — a mismatch (e.g., publishing a `bool` for a `StoneConfiguration` that expects `float`) just silently fails the requirement.
- **`activatorID` strings are the join key.** The same string in the activator and the requirement must match exactly (case-sensitive). Typos here are the most common reason a puzzle silently doesn't solve.
- **`PuzzleValidator` only knows the *latest* state.** The internal dictionary is overwritten on every event. There's no history, no debounce, no time window. A `TimedSwitch`-style requirement has to publish the final summary value (e.g., total held seconds) and the activator has to compute it.
- **One channel can serve many puzzles.** `activatorID` disambiguates them. Multiple channels are only useful if you want to scope events to a specific area or scene.
- **`onSolved` does not auto-undo on unsolved.** If you open a door on `onSolved`, you must explicitly close it on `onUnsolved` (or leave the trigger non-`reTriggerable`).
- **`reTriggerable = false` is per-`PuzzleValidator` lifetime.** `hasFired` is a runtime field on the `PuzzleTrigger`; it resets when the validator GameObject is recreated (e.g., on scene reload).
- **`GameEvents` is dead code from a teammate's perspective.** Don't add new code that calls it — write to `ActivatorStateChannel` instead. Removing `GameEvents` and migrating `LeverActivator` is a follow-up cleanup candidate.
- **`ActivatorState` struct is unused on the active path.** It exists for the legacy `GameEvents` overload. New code does not need it.
- **The deleted `Assets/Systems/Puzzle/*.md` files** described the pre-`ActivatorStateChannel` architecture (the static `GameEvents` path). They are superseded by this document; their templates are no longer correct.
