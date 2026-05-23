# Wiring a crystal (or any) Simon Says — step by step

Scripts live under **`Assets/Systems/TestingSOChannels/`** (`MusicalPuzzleStep`, `MusicalSequencePuzzleValidator`, `MusicalStepMaterialCue`, `InteractableTrigger`). This doc sits in **`Assets/Systems/MusicalPuzzle/`** next to melody / mushroom orchestration systems.

This flow uses **`MusicalPuzzleStep`** (per candle/crystal/pad) + **`MusicalSequencePuzzleValidator`** (order + failure replay). Everything shares one **`BoolActivatorChannel`** ScriptableObject asset.

---

## 1. Create shared assets (once per puzzle)

1. In the Project window: **Right‑click → Create → Events → Bool Activator Channel**. Name it e.g. `CrystalSimon_BoolChannel`.
2. Create one **`Activator ID`** ScriptableObject per crystal: **Create → Puzzle → Activator ID** (e.g. `CrystalSimon_A`, `CrystalSimon_B`, `CrystalSimon_C`).  
   - Asset **file names** are for humans; the validator compares **references** (the dragged-in assets), not the string alone.

---

## 2. Place the puzzle “brain” in the scene

1. Add an empty GameObject (e.g. **`CrystalSimon_Puzzle`**).
2. Add component **`MusicalSequencePuzzleValidator`**.
3. Assign **`State Channel`** = your shared `CrystalSimon_BoolChannel`.

---

## 3. Configure the solve order on the validator

1. **`Expected Order`**: press **+** once per step, **in solve order**.
2. For each slot, drag the **`ActivatorID`** asset for that step (first tap = slot 0, second = slot 1, …).
3. **`Puzzle Steps`**: drag each crystal’s **`MusicalPuzzleStep`** component reference (every crystal must appear here once, and **each step’s Activator ID** must match **one slot** in `Expected Order`; duplicate IDs are not supported in the replay map).

Optional tuning:

- **Failure — show correct melody**: leave **`Replay Correct Sequence On Failure`** on; adjust **hold** and **pause** seconds.
- **Completion**: **`Disable After Solve`** = ignore further taps after success.
- **Events**: **`On Sequence Solved`** / **`On Sequence Reset`** → doors, VO, animations, etc.

---

## 4. Set up each crystal GameObject

For **each** crystal (or bell, pad):

1. **Collider** suitable for triggers if you use proximity interact (often **`InteractableTrigger`** uses **`Is Trigger`** on its collider).
2. Add **`MusicalPuzzleStep`**.
3. On **`MusicalPuzzleStep`**:
   - **`State Channel`** = same `CrystalSimon_BoolChannel`.
   - **`Activator ID`** = **this crystal’s unique** `ActivatorID` asset (must match exactly one row in **`Expected Order`**).
   - **`Step Display Name`** = optional debug label only.
   - **`Audio Source`** + **`Interact Clip`** = one-shot tap sound (optional).
   - **`On Player Step Highlighted`**: things that happen on a **real** player tap — e.g. enable emissive mesh, pulse light, play particle (what you previously might have put on interact).
   - **`On Reset Presentation`**: idle / dim emissive — also used **between replay steps** after each flash.
4. Add **`InteractableTrigger`** on the crystal (same object or child — must have **`Collider`** with **Is Trigger** if you rely on proximity).
   - Under **`Interactable Trigger`→`On Interacted`** **(+)** → drag the **`MusicalPuzzleStep`** object → choose **`MusicalPuzzleStep` → `Interact()`**.
   - **Proximity behavior**: enable **`Activate On Player Trigger Enter`** for **touch** activation (runs **`On Interacted`** as soon as the Player enters — no **E**). Leave it off when you want **walk in + Interact/E** instead (shows optional prompt UI if you assigned a **`Interaction Prompt Channel SO`**).

Important: **`Interact()`** is what raises the bool channel to the validator. Do **not** call **`PlayLocalInteractFeedbackOnly()`** from the trigger — that skips validation.

### Material swap (dim ↔ bright)

`MusicalPuzzleStep` events are **no-argument** UnityEvents, so they cannot call `MaterialController.SetAllMaterials(bool)` by name alone. Use the bridge **`MusicalStepMaterialCue`** (same folder as **`MusicalPuzzleStep`**).

1. Add **`MaterialController`** on the crystal (often same GameObject as the mesh).
   - **`Object`** size ≥ 1: assign the crystal **`Renderer`** (or parent that owns all renderers you care about).
   - **`Start Material`** = idle / dim look.
   - **`Target Material`** = pressed / glowing look (`SetAllMaterials(true)` swaps to target).
   - Leave initial **`State`** unchecked (false = start material on `Init`).
2. Add **`MusicalStepMaterialCue`** on the **same GameObject or a convenient child**.
   - **`Material Controller`** → drag that **`MaterialController`** reference.
3. On **`MusicalPuzzleStep`**:
   - **`On Player Step Highlighted`** (+) → object with **`MusicalStepMaterialCue`** → **`MusicalStepMaterialCue → ApplyHighlightedMaterials()`**.
   - **`On Reset Presentation`** (+) → same component → **`ApplyIdleMaterials()`**.

That covers real taps, tutor replay flashes, and bulk reset after a mistake (all use the same events). If you hit **“locked”** warnings from **`MaterialController`**, unlock it in Inspector or avoid **`SetAllMaterialsAndLock`** on those crystals.

---

## 5. Test checklist

Play mode:

1. Wrong order → Console may log **`Wrong order`**; all **`Puzzle Steps`** get **`Reset`**, then the validator **plays the full sequence** (highlights ~1 s each by default) **without** counting as player input.
2. Correct full sequence → **`On Sequence Solved`** fires; with **`Disable After Solve`**, further taps are ignored.

If replay skips a lamp: that **`Activator ID`** isn’t paired with a **`MusicalPuzzleStep`** in **`Puzzle Steps`** (see Console warning).

---

## 6. What this does **not** use

This Simon path uses **`BoolActivatorChannel`** only. It does **not** require **`Mushroom`**, **`MushroomEventChannelSO`**, or **`MusicalSequenceConfiguration`** color arrays — those are for the older **tone/color melody + PuzzleValidator** pipeline. Crystals wired as above stay self-contained unless you deliberately hook **`On Sequence Solved`** into a larger **`PuzzleValidator`** puzzle.
