# Dialogue System Documentation

## Core Pieces

### DialogueSO

`DialogueSO` is the conversation asset. Each asset contains an ordered list of `DialogueLine` entries. A line stores:

- `speaker`: an optional `NPCSpeakerSO`. If this is empty, the runner reuses the most recent non-empty speaker.
- `text`: the dialogue text shown in the UI bubble.
- `autoAdvanceDelay`: a delay in seconds before the player is allowed to advance past that line.

The asset also has playback settings:

- `playOnce`: prevents the same trigger from starting this conversation again after it has finished once.
- `retriggerCooldown`: waits this many seconds after the conversation ends before the trigger can start it again.

Create one from the Unity menu:

```text
Create -> Dialogue -> Dialogue
```

### NPCSpeakerSO

`NPCSpeakerSO` stores reusable speaker identity data:

- display name
- optional portrait sprite
- speaker name color
- optional typing blip audio clip
- typing blip volume

Multiple dialogue assets can reuse the same speaker. If the character name, portrait, or color changes, update the speaker asset once and every conversation using it will pick up the change.

Create one from the Unity menu:

```text
Create -> Dialogue -> NPC Speaker
```

### DialogueEventChannelSO

`DialogueEventChannelSO` is the typed start event. A trigger raises this channel with a `DialogueSO` payload, and the scene's `DialogueRunner` listens for it.

It inherits from the generic project event channel in `Assets/Scripts/Events/EventChannelSO.cs`.

Create one from:

```text
Create -> Dialogue -> Events -> Dialogue Event Channel
```

### DialogueEndedChannelSO

`DialogueEndedChannelSO` is the "conversation finished" event. `DialogueRunner` raises it when the active dialogue ends.

`DialogueTrigger` listens to this event so it can reset its active state, mark `playOnce` conversations as used, and start cooldown timing. Other systems can also subscribe to this event if they need to resume player movement, camera control, AI, music, or interaction prompts.

Create one from:

```text
Create -> Dialogue -> Events -> Dialogue Ended Channel
```

### DialogueTrigger

`DialogueTrigger` goes on an NPC or a child trigger collider. It is responsible for detecting the player and requesting a conversation.

Inspector fields:

- `Dialogue`: the `DialogueSO` conversation to play.
- `Start Channel`: the shared `DialogueEventChannelSO` asset.
- `Ended Channel`: the shared `DialogueEndedChannelSO` asset.
- `Player Tag`: the tag required on the entering collider, usually `Player`.
- `Require Enter`: if true, starts on `OnTriggerEnter`; if false, also checks `OnTriggerStay`, which is useful if the player can spawn inside the trigger.

When the player enters, it checks:

- the collider has the required player tag
- no conversation from this trigger is currently active
- the assigned dialogue is valid
- `playOnce` has not already been consumed
- any cooldown has finished

If everything passes, it creates a `StartDialogueCommand` and executes it. The command raises the start channel.

### DialogueRunner

`DialogueRunner` is the scene-level controller. There should normally be one active runner in the scene.

It listens to the start channel, owns the currently active `DialogueSO`, advances line by line, runs the typewriter reveal coroutine, handles the advance key, and raises the ended channel when the conversation finishes.

Important behavior:

- If a conversation is already running, new start requests are ignored.
- The default advance key is `E`.
- Pressing the advance key while text is still revealing skips the reveal and shows the full line.
- Pressing again after the reveal is complete moves to the next line.
- If `autoAdvanceDelay` is set on the current line, the runner blocks advancing until the delay passes.
- When the final line is dismissed, the runner hides the UI and raises `DialogueEndedChannelSO`.

Inspector fields:

- `Start Channel`: the shared `DialogueEventChannelSO`.
- `Ended Channel`: the shared `DialogueEndedChannelSO`.
- `UI`: the scene's `DialogueUI`.
- `Advance Key`: keyboard key used to skip or advance.
- `Characters Per Second`: typewriter reveal speed. Set to `0` to disable the typewriter effect.

### DialogueUI

`DialogueUI` is a passive view. It does not decide which line plays or when the conversation advances. It only exposes methods that `DialogueRunner` calls:

- `Show()`
- `Hide()`
- `SetSpeaker(NPCSpeakerSO speaker)`
- `SetLine(string text)`
- `SetContinueIndicatorVisible(bool visible)`
- `PlayBlip(AudioClip clip, float volume)`

Attach it to a dialogue panel under a Canvas and wire the TextMeshPro fields in the Inspector.

Common fields:

- `Root`: object toggled on/off when dialogue starts and ends. If empty, the component's own GameObject is used.
- `Speaker Name Text`: TextMeshProUGUI for the speaker name.
- `Body Text`: TextMeshProUGUI for the dialogue body.
- `Portrait Image`: optional portrait Image.
- `Continue Indicator`: optional arrow or prompt shown after a line finishes revealing.
- `Audio Source`: optional source for typing blips.
- Font, size, outline, body color, and speaker gradient style settings.

### Commands

The system uses `IDialogueCommand`.

Existing commands:

- `StartDialogueCommand`: raises the start channel with a `DialogueSO`.
- `AdvanceDialogueCommand`: calls `DialogueRunner.TryAdvance()`.
- `SkipRevealCommand`: calls `DialogueRunner.CompleteCurrentReveal()`.
- `EndDialogueCommand`: calls `DialogueRunner.EndConversation()`.

This keeps callers from reaching directly into the runner or event channels. A trigger, UI button, cutscene, quest, or other system can create and execute a command without needing to know the internal dialogue flow.

## Runtime Flow

```text
Player enters trigger collider
-> DialogueTrigger validates tag, playOnce, cooldown, and assigned assets
-> DialogueTrigger executes StartDialogueCommand
-> StartDialogueCommand raises DialogueEventChannelSO with a DialogueSO
-> DialogueRunner receives the start request
-> DialogueRunner shows DialogueUI and starts the first line
-> DialogueRunner reveals text one character at a time
-> Player presses E to skip reveal or advance
-> DialogueRunner moves through all DialogueLine entries
-> DialogueRunner hides DialogueUI
-> DialogueRunner raises DialogueEndedChannelSO
-> DialogueTrigger records completion and starts cooldown if needed
```

## How To Set Up Dialogue In A Scene

### 1. Create shared event channel assets

If they do not already exist, create these assets somewhere stable, such as `Assets/Systems/Dialogue/`:

```text
Create -> Dialogue -> Events -> Dialogue Event Channel
Create -> Dialogue -> Events -> Dialogue Ended Channel
```

This project already has:

- `Assets/Systems/Dialogue/DialogueEventChannel.asset`
- `Assets/Systems/Dialogue/DialogueEndedChannel.asset`

Use the same shared assets for the scene runner and all dialogue triggers in that scene.

### 2. Create a speaker asset

Create a speaker:

```text
Create -> Dialogue -> NPC Speaker
```

Fill in:

- `Display Name`
- optional `Portrait`
- `Name Color`
- optional `Typing Blip`
- `Typing Blip Volume`

This project already includes:

- `Assets/Systems/Dialogue/Speaker_Robot.asset`

### 3. Create a dialogue asset

Create a conversation:

```text
Create -> Dialogue -> Dialogue
```

Add lines to the `Lines` list. For each line:

- assign a speaker, or leave it empty to reuse the previous speaker
- write the line text
- optionally set `Auto Advance Delay`

Set playback options:

- enable `Play Once` for one-time story conversations
- set `Retrigger Cooldown` for repeatable NPC chatter

This project already includes:

- `Assets/Systems/Dialogue/Dialogue_Robot_Intro.asset`

### 4. Add the DialogueRunner

Create an empty GameObject in the scene, commonly named `DialogueRunner`, and add the `DialogueRunner` component.

Assign:

- `Start Channel`: `DialogueEventChannel.asset`
- `Ended Channel`: `DialogueEndedChannel.asset`
- `UI`: the scene's `DialogueUI` component
- `Advance Key`: usually `E`
- `Characters Per Second`: for example `40`

### 5. Build the dialogue UI panel

Under a Canvas, create a dialogue panel with:

- TextMeshProUGUI for speaker name
- TextMeshProUGUI for body text
- optional Image for portrait
- optional continue indicator
- optional AudioSource for typing blips

Add `DialogueUI` to the panel and wire those fields.

The panel can start disabled. `DialogueUI.Show()` will activate it when a conversation begins.

### 6. Add a trigger to an NPC

On the NPC or a child object:

1. Add a Collider.
2. Enable `Is Trigger`.
3. Add `DialogueTrigger`.
4. Assign the desired `DialogueSO`.
5. Assign the same shared start and ended channel assets used by `DialogueRunner`.
6. Make sure the player collider has the tag listed in `Player Tag`.

When the player enters the trigger, the dialogue should start.

## Current Example

The existing robot intro is set up through:

- `Speaker_Robot.asset`: defines the Robot speaker name and cyan name color.
- `Dialogue_Robot_Intro.asset`: contains two Robot lines, `playOnce` disabled, and a 5 second retrigger cooldown.
- `DialogueEventChannel.asset`: shared start channel.
- `DialogueEndedChannel.asset`: shared completion channel.

Use this as the reference setup when adding another NPC conversation.
