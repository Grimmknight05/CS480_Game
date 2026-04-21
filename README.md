# CS-480 Game

Documentation
---

## PlayerController

**By Joshua Henrikson**

## PlayerController.cs

Handles player movement, camera control, scoring, UI, and interactions using Unity's Input System and Rigidbody physics.

---

## Core Systems

### Movement System

Supports multiple movement modes that can be dynamically changed at runtime.

#### Main Movement Modes

* **AccelerationBased (Ground)**

  * Smooth, responsive movement
  * Camera-relative (XZ plane)
  * Uses acceleration/deceleration for control

* **ZeroGravity**

  * Full 6DOF movement (forward, strafe, vertical)
  * Camera-relative (includes up vector)
  * No gravity, free-floating motion
  * Separate acceleration/deceleration tuning

#### Mode Switching

* Controlled externally via **Movement Zones**
* Player has a **default movement mode**
* Automatically reverts when leaving a zone

---

### Movement Zones

Environmental triggers that modify player movement behavior.

#### Behavior

* On enter → sets player movement mode
* On exit → restores default mode

#### Use Cases

* Gravity zones (interiors)
* Zero gravity zones (space)
* Can get creative with these, maybe we can have futuristic Zero-g zones and gravity zones on planets
* Good more mechanic
* Potental Future expansion:

  * Low gravity
  * Water/swimming?
  * Slow/mud zones?

---

### Camera System

* Mouse-based look input
* Horizontal rotation affects player body
* Vertical rotation affects camera pivot
* Clamped pitch for stability

---

### Jump System (Ground Mode Only)

* Ground jump + air jumps
* Raycast-based ground detection
* Separate jump forces for ground and air jumps
* Can do an air jump after falling off a platform
* Disabled in ZeroGravity mode

---

### Score System

* Tracks player points
* Updates UI (TextMeshPro)
* Fires event on change:

```csharp
OnScoreChanged(int newScore)
```

---

### Collision & Interaction

* **PickUp**

  * Adds score
  * Triggers pickup behavior

* **Enemy**

  * Triggers death state
  * Fires event:

```csharp
OnPlayerDeath()
```

---

### UI System

* Score display
* Win screen
* Death screen

---

### Audio System

* Randomized jump and air jump SFX
* Uses AudioSource component

---

## Required Components

* Rigidbody
* PlayerInput
* Camera (with pivot)
* TextMeshProUGUI
* AudioSource

---

## Input Actions
New input system

* **Move** → Vector2 (WASD / stick)
* **Look** → Vector2 (mouse)
* **Jump** → Button

---

## Events

```csharp
OnScoreChanged(int newScore)
OnPlayerDeath()
```

---

## Planets

Custom planets created in Blender and textured in Substance Painter.
Added custom atmosphere shader

Potential future features:
