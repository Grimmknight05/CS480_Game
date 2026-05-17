## Strict Editor & Version Control Constraints

**The ".meta File" Principle (Refactor in Place):** NEVER instruct the user to delete an existing script and create a new one with a different name in a new folder if the script serves the same purpose. Always provide code that can be pasted directly into the existing file. If a file needs to be moved, explicitly instruct the user to "Move the file from within the Unity Editor Project window."

**The "Inspector Wipe" Principle (Serialized Type Changes):** Whenever changing the type or name of a `[SerializeField]` or public variable, issue a bolded **EDITOR WARNING:** instructing the user to document their current scene setups before compiling.

**The "Downstream Dependency" Principle (Save Data & UI):** Before changing a foundational data identifier, analyze downstream systems. Propose necessary serialization bridges if switching to ScriptableObject references so JSON/Binary save files do not break.

**The "Scene Freeze" Principle (Version Control):** When proposing massive architectural refactors to core GameObjects or MonoBehaviours, remind the user to call a "Scene/Prefab Freeze" with the team to avoid Git merge conflicts.

## Architectural Boundaries

### Physics Constraints

- **Zero-Friction & Slope Adhesion Integrity:** The custom character controller relies on a decoupled State Pattern with "Zero Friction" proxy colliders and specific raycast/spherecast slope adhesion logic. You MUST NEVER suggest messy `rb.drag` workarounds or native Unity friction materials that conflict with this architecture.
    
- **Rigidbody Teleportation & Checkpoints:** NEVER snap a Rigidbody directly using `transform.position`. You MUST explicitly zero out `rb.velocity` and `rb.angularVelocity`, and use `rb.position = targetPos` or `rb.MovePosition()` to ensure PhysX registers the teleportation safely before the first physics tick.
    
- **Spawn Point Safety:** Checkpoint or teleport logic MUST NEVER record collision intersection coordinates. Rely on explicitly placed empty child `Transform` objects (e.g., `safeSpawnPoint`) to prevent floor clipping or embedding upon scene reload.
    
- **Layer Matrix Constraints:** Prevent "Skyrim Horse" jump-spamming exploits. Steep cliff rock materials and colliders MUST remain explicitly excluded from the `Jumpable` LayerMask in `OnCollisionEnter` physics checks.
    

### Event Systems

- **ScriptableObject (SO) Event Channels:** You MUST use SO Event Channels as the primary communication backbone (Observer Pattern). Broadcasters (e.g., triggers, activators) MUST NOT hold direct references to Listeners (e.g., UI, doors).
    
- **Prevent Ghost Listeners:** Any MonoBehaviour subscribing to an SO Event Channel MUST subscribe in `OnEnable` and unsubscribe in `OnDisable`. Failure to do so causes memory leaks and cross-scene ghost execution.
    
- **Editor Persistence Protocol:** ScriptableObjects mutate permanently in Editor Play Mode. Any SO carrying transient runtime data (like `PlayerSessionData`) MUST implement an `OnEnable` method that wipes dirty data (e.g., `hasCheckpoint = false`) to guarantee clean initialization per session.
    
- **Puzzle Validation:** The Puzzle System strictly follows a 3-layer decoupled format: `Activators` (MonoBehaviours) broadcast to `ActivatorStateChannel` (SO), which is monitored by `PuzzleValidator` (MonoBehaviour). Adhere strictly to this interface pipeline (`IActivatorRequirement`).
    

### Design Patterns

- **State Pattern:** Do not use massive `Update()` blocks with nested `if/else` checks for player or AI logic. Extract discrete behaviors into State objects (e.g., `MovementState`, `GroundedMovementState`, `ZeroGMovementState`) and execute via `currentState.Tick()` and `FixedTick()`.
    
- **Command Pattern:** Raw inputs MUST NOT force immediate mechanical execution. Inputs must generate Commands (e.g., `JumpCommand`) placed in an input buffer/queue to allow for input buffering, coyote time, and undo/redo histories.
    
- **Component Pattern (Composition > Inheritance):** NEVER build deep monolithic inheritance trees (e.g., `GameObject` -> `PuzzleObject` -> `Door` -> `MovingPuzzleDoor`). Build blank container entities and attach discrete logic components (`TransformComponent`, `ActivatorReceiverComponent`).
    
- **Subclass Sandbox:** When designing varied entity behaviors (like abilities or specific puzzle interactables), provide protected utility methods in the base class. Subclasses MUST use these sandboxed methods rather than calling external systems directly.
    
- **Spatial Partitioning:** When scaling systems that iterate over many entities (e.g., checking distances of 50+ enemies or items), you MUST implement a Grid or Quadtree Spatial Partition to prevent $O(N^2)$ frame drops. Do not default to global iteration.

-  **Flyweight Pattern:** NEVER instantiate duplicate intrinsic data (meshes, base stats, shared textures) for swarms of enemies, particles, or repeating environment objects (e.g., trees, pillars). You MUST share intrinsic state via ScriptableObjects or static references, isolating only the extrinsic data (position, current health, rotation) on the individual instances to preserve memory.
    
- **Observer Pattern (Strict Decoupling):** NEVER tightly couple cross-domain systems (e.g., the Player script directly calling `UIManager.UpdateHealth()` or `AudioManager.PlaySound()`). You MUST use the Observer Pattern to broadcast state changes. If you suggest using `GameObject.Find()` or `GetComponent<UIController>()` from a physics entity, you are violating this architecture.
    
- **Update Method / Sequencing:** DO NOT build monolithic global loops that explicitly hardcode behavior for every entity (e.g., a massive `for` loop in a GameController moving every skeleton). Entities MUST implement their own isolated `Tick()` or `Update()` logic, which is sequentially driven by the broader game loop, keeping entity behaviors self-contained.
    
- **Singleton Constraints:** AVOID MonoBehaviour Singletons for general game logic. If global access is strictly required, you MUST first evaluate if a ScriptableObject-based architecture (like Event Channels) can replace it. If a Singleton is absolutely unavoidable (e.g., a core bootstrapper), it MUST NOT hold transient, scene-specific data that will cause ghost bugs upon scene reloads.