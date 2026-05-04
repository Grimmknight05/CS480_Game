# Orbital Camera Overview

The Orbital Camera is a self-contained third-person camera controller. It orbits a target Transform using mouse input (yaw/pitch), keeps a fixed distance from the target unless a wall is in the way (in which case it sphere-casts and snaps in front of the wall), and locks the cursor on start. It is **not** part of the SO/event-channel architecture used elsewhere in the project — see **Notes** below.

## Core Components

- **[`OrbitalCamera`](../Assets/Scripts/DavidPocScripts/UnifiedOrbitalCamera.cs)** — single `MonoBehaviour`. Attach to the camera GameObject. The class is named `OrbitalCamera` even though the file is `UnifiedOrbitalCamera.cs`; this mismatch is a known cleanup candidate (see Notes).

That's the entire system. There are no SOs, no channels, and no other scripts involved.

## The Event Flow

There are no events. The camera runs entirely off `Update`-style polling:

1. `Start()` — locks and hides the cursor (`Cursor.lockState = Locked`, `Cursor.visible = false`); seeds `currentYaw` from `playerRef.eulerAngles.y` so the camera starts behind the player; sets `currentPitch = startingPitch`.
2. `LateUpdate()` (every frame, after all `Update` calls):
   1. Read `Mouse.current.delta` from the new Input System.
   2. Add `mouseX * yawSpeed` to `currentYaw`. Subtract `mouseY * pitchSpeed` from `currentPitch`.
   3. Clamp `currentPitch` to `[minPitch, maxPitch]`.
   4. Build `Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0)`.
   5. Compute `targetPosition = playerRef.position + targetOffset` (the "look-at" point — head/shoulders).
   6. Compute `desiredPosition = targetPosition + rotation * new Vector3(0, 0, -distance)` (where the camera *wants* to be).
   7. `Physics.SphereCast` from `targetPosition` toward `desiredPosition` with radius `cameraRadius`, masked by `collisionLayers`.
      - If it hits, place the camera at `hit.point + hit.normal * 0.1f` (small offset prevents near-clipping the wall).
      - If it doesn't, place the camera at `desiredPosition`.
   8. Set `transform.rotation = rotation`.

`LateUpdate` early-outs if `playerRef` is null.

## Editor Setup & Wiring

1. Create the camera. Either use the Main Camera or a new GameObject with a `Camera` component.
2. Attach `OrbitalCamera` to that GameObject.
3. Drag the **player Transform** into the `Player Ref` slot.
4. Set `Collision Layers` to whichever physics layers the camera should collide with (typically `Default` plus your environment layers — *not* the player's own layer).
5. Tune the rest from defaults. Sensible starting values are already in the script.

### Inspector fields (grouped exactly as `[Header]` in the script)

| Header | Field | Type | Default | Purpose |
|---|---|---|---|---|
| Camera Collision | `collisionLayers` | `LayerMask` | — | Which layers the SphereCast collides with. |
| Camera Collision | `cameraRadius` | `float` | `0.3` | SphereCast radius — how "thick" the camera is. |
| Targeting | `playerRef` | `Transform` | — | The orbit target. Required. |
| Targeting | `targetOffset` | `Vector3` | `(0, 1.5, 0)` | Look-at offset from `playerRef.position` (aims at shoulders/head, not feet). |
| Camera Distance | `distance` | `float` | `5` | Current desired distance from the target. |
| Camera Distance | `minDistance` | `float` | `2` | **Currently unused** — see Notes. |
| Camera Distance | `maxDistance` | `float` | `10` | **Currently unused** — see Notes. |
| Orbit Speeds | `yawSpeed` | `float` | `0.2` | Multiplier on horizontal mouse delta. |
| Orbit Speeds | `pitchSpeed` | `float` | `0.2` | Multiplier on vertical mouse delta. |
| Pitch Limits | `minPitch` | `float` | `-20` | Lower clamp on pitch (degrees). |
| Pitch Limits | `maxPitch` | `float` | `60` | Upper clamp on pitch (degrees). |
| Starting Perspective | `startingPitch` | `float` | `20` | Pitch the camera starts at on `Start()`. |

## Code Examples / Templates

This script is not an architectural template — see Notes below — so there's no "extend it" pattern to mirror. The two realistic things a teammate might want to do:

### Add scroll-wheel zoom (uses the currently-unused min/max distance fields)

```csharp
[Header("Zoom")]
public float zoomSpeed = 1f;

void LateUpdate()
{
    // ... existing code ...

    Mouse m = Mouse.current;
    if (m != null)
    {
        float scroll = m.scroll.ReadValue().y;
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        // ... existing yaw/pitch reads ...
    }
}
```

This would give `minDistance` and `maxDistance` an actual job. Today they're just exposed Inspector values that nothing reads.

### Disable mouse capture for menus

`Cursor.lockState = CursorLockMode.Locked` is set in `Start()` and never released. If you add pause menus, you'll want a public method (or a subscriber on a `MenuOpenedChannelSO`-style event) to flip the cursor lock state on/off. Not currently implemented.

## Notes / Caveats

- **Architectural outlier — do not use as a template.** This script is a self-contained `MonoBehaviour` with no `ScriptableObject` references and no event channels. The rest of the project uses the SO + channel pattern (see [`EventChannels.md`](EventChannels.md) and [`TransitionSystem.md`](TransitionSystem.md)). New gameplay systems should follow those, not this one.
- **Class/file name mismatch.** The class is `OrbitalCamera`, the file is `UnifiedOrbitalCamera.cs`. Unity tolerates this for non-MonoBehaviour types but flags it as a warning for `MonoBehaviour`s. Cleanup candidate: rename the file to `OrbitalCamera.cs` (and rename its `.meta` alongside) **or** rename the class to `UnifiedOrbitalCamera`.
- **`minDistance` and `maxDistance` are unused.** They appear in the Inspector but nothing in `LateUpdate` reads them. Either wire them up (see the zoom snippet above) or remove them — currently they mislead a designer into thinking distance is clamped.
- **Requires the new Input System.** The script uses `UnityEngine.InputSystem` and `Mouse.current`. The project must have the new Input System enabled in **Project Settings → Player → Active Input Handling** (either "Input System Package" or "Both"). The legacy `Input.GetAxis("Mouse X")` API is not used.
- **Cursor stays locked for the lifetime of the scene.** No unlock path is exposed. If you need to release the mouse for UI, you'll need to add one.
- **Collision uses world-space SphereCast from the *target*, not from the camera's last position.** This means the camera teleports to its corrected position each frame rather than easing — it can feel snappy. Smoothing (e.g., `Vector3.SmoothDamp`) is not implemented; flag as a polish candidate.
- **`playerRef` is read in `Start()` to seed yaw, but `Start()` does not null-check it.** A missing `playerRef` will throw on the first scene load. `LateUpdate` does null-check. Setting the Inspector reference is mandatory.
