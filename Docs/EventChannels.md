# Event Channels Overview

Event Channels are the project's foundational decoupling mechanism: a publisher object raises a signal on a `ScriptableObject` asset, and any number of subscriber objects react to it without either side holding a direct reference to the other. The base type is `VoidEventChannelSO` — a parameterless broadcast — and each concrete channel type is its own one-line subclass that exists as an `.asset` file in the project.

This is the pattern teammates should reach for whenever **"system A needs to tell system B something happened"** and the two systems should not know about each other directly.

## Core Components

- **[`VoidEventChannelSO`](../Assets/Scripts/Events/VoidEventChannelSO.cs)** — abstract base `ScriptableObject`. Owns:
  - `public event Action OnRaised` — the C# event subscribers attach to.
  - `public void Raise()` — call this from a publisher to fire the event.
  - `void OnDisable()` — sets `OnRaised = null` when the SO is unloaded. This is intentional: it prevents subscribers from leaking across Play-Mode entries when Unity's Domain Reload is disabled.
- **Concrete channel types** — empty subclasses with a `[CreateAssetMenu]` attribute. They exist purely so each kind of event has its own asset file and its own Inspector slot type. Current concrete subclasses in the project:
  - [`PlayerDeathChannelSO`](../Assets/Scripts/DavidPocScripts/Transition Scripts/PlayerDeathChannelSO.cs)
  - [`FuelCollectedChannel`](../Assets/Scripts/Events/FuelCollectedChannel.cs)
  - [`DialogueEndedChannelSO`](../Assets/Scripts/Dialogue/DialogueEndedChannelSO.cs)
- **Channel asset files** — `.asset` files created from the `[CreateAssetMenu]` entries. The asset itself carries no per-instance data; it's a stable reference handle that publishers and subscribers both point at via `[SerializeField]` fields.

## The Event Flow

1. A publisher MonoBehaviour holds a `[SerializeField]` reference to a channel SO.
2. Some condition fires (e.g., a trigger collider, a button press, a timer).
3. The publisher calls `channel.Raise()`.
4. `Raise()` invokes `OnRaised?.Invoke()`.
5. Every subscriber that did `channel.OnRaised += MyHandler` runs, in subscription order.
6. On Play-Mode exit, `OnDisable()` clears the subscriber list so the next session starts clean.

There is no priority, no ordering guarantee beyond C# delegate invocation order, and no data payload — `VoidEventChannelSO` is a "something happened" pulse, not a message bus.

## Editor Setup & Wiring

To stand up a new channel and use it:

1. **Create the channel script.** Add a one-line subclass anywhere under `Assets/Scripts/`:
   ```csharp
   using UnityEngine;

   [CreateAssetMenu(menuName = "Events/My Event Channel", fileName = "MyEvent.channel.asset")]
   public class MyEventChannelSO : VoidEventChannelSO { }
   ```
2. **Create the asset.** In the Unity **Project** window, right-click → **Create → Events → My Event Channel**. This produces a `MyEvent.channel.asset` file. Place it somewhere predictable like `Assets/Events/`.
3. **Wire the publisher.** On the script that should fire the event, declare a `[SerializeField]` field of the new channel type. Drag the asset onto that field in the Inspector. Call `channel.Raise()` when you want the event to fire.
4. **Wire the subscriber.** On the script that should react, declare the same `[SerializeField]` channel field. Drag the *same* asset into it. Subscribe in `OnEnable` and unsubscribe in `OnDisable`.

A single channel asset can have many publishers and many subscribers — that's the point of the pattern.

## Code Examples / Templates

### Template: declare a new channel type

Mirror [`PlayerDeathChannelSO`](../Assets/Scripts/DavidPocScripts/Transition Scripts/PlayerDeathChannelSO.cs):

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Boss Defeated Channel", fileName = "BossDefeated.channel.asset")]
public class BossDefeatedChannelSO : VoidEventChannelSO { }
```

The body stays empty. The whole point of a separate type is to give Unity a distinct Inspector slot so a designer can't accidentally drag a death channel into a boss-defeated field.

### Template: publisher

```csharp
using UnityEngine;

public class BossDeathTrigger : MonoBehaviour
{
    [SerializeField] private BossDefeatedChannelSO bossDefeatedChannel;

    public void OnBossHpZero()
    {
        if (bossDefeatedChannel != null) bossDefeatedChannel.Raise();
    }
}
```

### Template: subscriber

Always pair `OnEnable` / `OnDisable` so handlers don't leak when the GameObject is disabled or destroyed:

```csharp
using UnityEngine;

public class VictoryMusicPlayer : MonoBehaviour
{
    [SerializeField] private BossDefeatedChannelSO bossDefeatedChannel;

    void OnEnable()
    {
        if (bossDefeatedChannel != null)
            bossDefeatedChannel.OnRaised += HandleBossDefeated;
    }

    void OnDisable()
    {
        if (bossDefeatedChannel != null)
            bossDefeatedChannel.OnRaised -= HandleBossDefeated;
    }

    void HandleBossDefeated()
    {
        // play victory sting
    }
}
```

## When to Use This Pattern

Use a channel when:

- The publisher and subscriber live in different scenes, prefabs, or systems and shouldn't reference each other directly.
- Multiple unrelated systems need to react to the same moment (death, dialogue end, item pickup, etc.).
- A designer needs to wire reactions in the Editor without writing code.

Don't use a channel when:

- The two objects are already part of the same prefab/system and a direct reference is simpler.
- You need to pass data with the event — `VoidEventChannelSO` is parameterless. See **Notes** below.

## Notes / Caveats

- **Void only.** There is no typed/parameterized event channel in the project today. If you need to pass a payload (a damage amount, a scored item, a string), that's a future-implementation candidate — currently it would mean writing a new abstract base (e.g., `IntEventChannelSO`) parallel to `VoidEventChannelSO`.
- **`OnDisable` clears subscribers.** The base class wipes `OnRaised` when the SO unloads; the relevant behavior is that this guards against leaked handlers when Unity's Domain Reload is turned off in Project Settings → Editor → Enter Play Mode Options. You don't need to do anything special — just be aware that subscribers must re-attach in `OnEnable`.
- **No invocation-order guarantees** beyond the order subscribers were added. Don't rely on subscriber A running before subscriber B.
- **Null-check the channel field** in both publishers and subscribers (the existing scripts do). A missing asset reference is the single most common wiring mistake.
