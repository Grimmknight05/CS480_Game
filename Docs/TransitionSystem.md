# Transition System Overview

The Transition System handles **player death and respawn**. When the player enters a hazard, an event channel is raised; a manager listening on that channel reloads the active scene; on respawn, the player is teleported back to the most recent checkpoint position stored in a shared `ScriptableObject`. The system is fully decoupled — hazards, checkpoints, the respawn manager, and the spawn positioner never reference each other directly. They only share two assets: a `PlayerDeathChannelSO` and a `PlayerSessionData` SO.

## Core Components

All scripts live under [`Assets/Scripts/DavidPocScripts/Transition Scripts/`](../Assets/Scripts/DavidPocScripts/Transition Scripts/).

**State container (data SO)**
- **[`PlayerSessionData`](../Assets/Scripts/DavidPocScripts/Transition Scripts/PlayerSessionData.cs)** — `ScriptableObject` storing the most recent checkpoint as `Vector3 lastCheckpoint` plus a `bool hasCheckpoint` flag. Exposes `SetCheckpoint(Vector3)`, `ClearCheckpoint()`, and `TryGetCheckpoint(out Vector3)`. Its `OnEnable()` resets both fields on every Play-Mode entry — this is intentional (see Caveats).

**Event channel**
- **[`PlayerDeathChannelSO`](../Assets/Scripts/DavidPocScripts/Transition Scripts/PlayerDeathChannelSO.cs)** — concrete `VoidEventChannelSO` subclass. Created via **Create → Events → Player Death Channel**. See [`EventChannels.md`](EventChannels.md) for the underlying pattern.

**Publisher (raises the death channel)**
- **[`WaterHazard`](../Assets/Scripts/DavidPocScripts/Transition Scripts/WaterHazard.cs)** — trigger volume. On `OnTriggerEnter` from a tagged player, calls `deathChannel.Raise()`. Auto-forces its collider to `IsTrigger = true` in `Awake` and warns if it had to.

**Subscriber (handles death)**
- **[`GameLoopManager`](../Assets/Scripts/DavidPocScripts/Transition Scripts/GameLoopManager.cs)** — subscribes to `PlayerDeathChannelSO.OnRaised` in `OnEnable`, unsubscribes in `OnDisable`. On death, reloads the active scene via `SceneManager.LoadScene(activeBuildIndex)`.

**Checkpoint writer**
- **[`CheckpointVolume`](../Assets/Scripts/DavidPocScripts/Transition Scripts/CheckpointVolume.cs)** — trigger volume. On player entry, writes `safeSpawnPoint.position` into `PlayerSessionData`. Supports a `oneShot` flag that disables further writes after the first hit (instance-local; survives until scene reload).

**Checkpoint reader / spawn placer**
- **[`PlayerSpawnPositioner`](../Assets/Scripts/DavidPocScripts/Transition Scripts/PlayerSpawnPositioner.cs)** — attached to the player. In `Start()`, asks `PlayerSessionData.TryGetCheckpoint`. If a checkpoint exists, teleports the player there (zeroing Rigidbody linear and angular velocity first). If not, records the player's current position as the initial checkpoint.

## The Event Flow

The full death/respawn cycle, end to end:

1. **Checkpoint capture.** Player walks through a `CheckpointVolume` trigger →
   `CheckpointVolume.OnTriggerEnter` → `PlayerSessionData.SetCheckpoint(safeSpawnPoint.position)`.
2. **Hazard entry.** Player walks into a `WaterHazard` trigger →
   `WaterHazard.OnTriggerEnter` → `PlayerDeathChannelSO.Raise()`.
3. **Channel broadcast.** `Raise()` invokes `OnRaised?.Invoke()` on the channel SO.
4. **Scene reload.** `GameLoopManager.HandlePlayerDeath` runs →
   `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`.
5. **Respawn placement.** The reloaded player object's `PlayerSpawnPositioner.Start()` runs →
   `PlayerSessionData.TryGetCheckpoint(out pos)` returns true →
   `SafeTeleport(pos)` zeros the Rigidbody's `linearVelocity` and `angularVelocity`, then sets `rb.position` and `transform.position` to the checkpoint.

If the player dies before ever touching a `CheckpointVolume`, step 5 takes the `else` branch: the positioner records `transform.position` as the initial checkpoint, which means subsequent deaths return the player to wherever the player object was originally placed in the scene.

## Editor Setup & Wiring

This is the full setup for a new scene that uses the Transition System. Do this once per scene; the SO assets can (and should) be reused across scenes.

### One-time asset creation

1. In the **Project** window, right-click → **Create → Events → Player Death Channel**. Name it `PlayerDeath.channel.asset`. Place under `Assets/Events/` (or your channels folder).
2. Right-click → **Create → Session → Player Session Data**. Name it `PlayerSessionData.asset`. Place under `Assets/Session/` (or similar).

Both assets are project-wide singletons — every scene drags the *same* assets into its slots.

### Per-scene wiring

3. **Manager GameObject.** Create an empty GameObject (e.g., `_GameLoop`). Attach `GameLoopManager`. Drag `PlayerDeath.channel.asset` into the **Death Channel** slot.
4. **Player object.** On your player root, attach `PlayerSpawnPositioner`. Drag `PlayerSessionData.asset` into the **Session Data** slot. Ensure the player has a `Rigidbody` if you want velocity zeroing on respawn (the script null-checks for one).
5. **Hazards.** For each kill volume (water, lava, pit, etc.):
   - Add a `Collider` (any shape). The script will force `IsTrigger = true` at runtime if you forget — but set it explicitly in the Editor to avoid the warning.
   - Attach `WaterHazard`.
   - Drag `PlayerDeath.channel.asset` into the **Death Channel** slot.
   - Confirm the `Player Tag` field matches the tag on your player object (default `"Player"`).
6. **Checkpoints.** For each checkpoint volume:
   - Create a parent GameObject with a trigger `Collider` placed where the player will walk through it.
   - As a child of that GameObject (or anywhere in the scene), place an empty `Transform` named `SafeSpawnPoint` at the location the player should appear after dying.
   - On the parent, attach `CheckpointVolume`. Drag `PlayerSessionData.asset` into the **Session Data** slot. Drag the `SafeSpawnPoint` Transform into the **Safe Spawn Point** slot.
   - Set `One Shot` if this checkpoint should only ever fire once per scene load. Leave it off for repeatable checkpoints.

### Inspector field summary

| Component | Field | Purpose |
|---|---|---|
| `GameLoopManager` | `deathChannel` | The channel it subscribes to. |
| `PlayerSpawnPositioner` | `sessionData` | Where it reads the checkpoint from. |
| `WaterHazard` | `deathChannel` | The channel it raises on trigger. |
| `WaterHazard` | `playerTag` | Tag the trigger filters on. |
| `CheckpointVolume` | `sessionData` | Where it writes the checkpoint to. |
| `CheckpointVolume` | `safeSpawnPoint` | Transform whose `position` is recorded. |
| `CheckpointVolume` | `playerTag` | Tag the trigger filters on. |
| `CheckpointVolume` | `oneShot` | Disable further writes after first hit. |

## Code Examples / Templates

### Template: a new hazard type

To add a new kill volume (lava, spike pit, instant-death zone), copy the `WaterHazard` shape — only the class name needs to change:

```csharp
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LavaHazard : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private string playerTag = "Player";

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (deathChannel != null) deathChannel.Raise();
    }
}
```

All hazards in the scene drag the *same* `PlayerDeath.channel.asset` into their slot. The `GameLoopManager` doesn't need to know any of them exist.

### Template: hooking a new system into the death event

Anything that should react to death — a death sound, a fade-to-black, a stat counter — subscribes to the same channel. Example:

```csharp
using UnityEngine;

public class DeathCounter : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    public int deaths;

    void OnEnable()
    {
        if (deathChannel != null) deathChannel.OnRaised += HandleDeath;
    }

    void OnDisable()
    {
        if (deathChannel != null) deathChannel.OnRaised -= HandleDeath;
    }

    void HandleDeath() => deaths++;
}
```

## Notes / Caveats

- **Checkpoints do not persist across sessions.** `PlayerSessionData.OnEnable()` resets `hasCheckpoint` and `lastCheckpoint` on every Play-Mode entry. This is deliberate — SO field mutations during Play Mode otherwise dirty the `.asset` file in the Editor and would survive into the next session. **Treat `PlayerSessionData` as in-memory session state, not as save-game data.** A real save system is a future-implementation candidate.
- **Scene-reload model, not in-place respawn.** Death always reloads the active scene. Anything that wants to survive death (fuel counters, dialogue state) needs to live on a SO whose state survives a scene load — `PlayerSessionData` is the existing example. MonoBehaviour state on scene objects is wiped.
- **Trigger colliders are auto-forced.** Both `WaterHazard` and `CheckpointVolume` set `IsTrigger = true` in `Awake` and log a warning if they had to. Set it in the Editor to avoid the noise.
- **Velocity is zeroed on respawn.** `PlayerSpawnPositioner.SafeTeleport` zeros `linearVelocity` and `angularVelocity` before moving the Rigidbody, so a fall in progress doesn't carry into the respawn point. Players without a Rigidbody just get `transform.position` set.
- **`oneShot` is per scene-load, not permanent.** The `consumed` bool is an instance field on the `CheckpointVolume` MonoBehaviour. The GameObject is destroyed and recreated on scene reload, so a "one shot" checkpoint can fire again after a death.
- **No death animation, audio, or UI hook today.** The only listener is `GameLoopManager`. Adding a death sound, a hit-flash, or a "You Died" UI element is a matter of writing a new subscriber on `PlayerDeathChannelSO` — no changes needed to existing scripts.
