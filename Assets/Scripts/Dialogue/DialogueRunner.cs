// =====================================================================
// A single scene-level controller that owns the runtime flow of a
// conversation. Subscribes to the shared DialogueEventChannelSO so any
// NPC trigger can start a conversation. Iterates the lines in the
// supplied DialogueSO, drives the DialogueUI, and advances when the
// player presses the Interact action (already defined on the project's
// PlayerInput asset).
//
// While a conversation is active, additional start requests are
// ignored so two NPCs can't talk over each other. When the last line
// is dismissed, raises the DialogueEndedChannelSO so the player and
// other systems can resume normal behavior.
// =====================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DialogueRunner : MonoBehaviour
{
    [Header("Channels")]
    [Tooltip("Channel raised by NPC triggers to start a conversation.")]
    [SerializeField] private DialogueEventChannelSO startChannel;

    [Tooltip("Channel raised when the active conversation finishes.")]
    [SerializeField] private DialogueEndedChannelSO endedChannel;

    [Header("View")]
    [Tooltip("UI component that renders bubbles. Usually a child of a Canvas in the scene.")]
    [SerializeField] private DialogueUI ui;

    [Header("Input")]
    [Tooltip("Keyboard key that advances dialogue / skips the typewriter reveal. " +
             "Defaults to E. Uses the new Input System directly so no " +
             "InputActionReference wiring is required.")]
    [SerializeField] private Key advanceKey = Key.E;

    [Header("Reveal")]
    [Tooltip("Characters per second for the typewriter reveal. Set to 0 to disable.")]
    [Min(0f)]
    [SerializeField] private float charactersPerSecond = 40f;

    // Runtime state
    private DialogueSO active;
    private int lineIndex = -1;
    private Coroutine revealRoutine;
    private bool revealComplete = false;
    private bool advanceLockedUntilDelay = false;
    private float advanceUnlockTime = 0f;

    public bool IsRunning => active != null;

    void OnEnable()
    {
        if (startChannel != null) startChannel.OnRaised += HandleStartRequest;
    }

    void OnDisable()
    {
        if (startChannel != null) startChannel.OnRaised -= HandleStartRequest;
    }

    void Update()
    {
        if (!IsRunning) return;
        if (Keyboard.current == null) return;
        if (Keyboard.current[advanceKey].wasPressedThisFrame)
        {
            TryAdvance();
        }
    }

    private void HandleStartRequest(DialogueSO dialogue)
    {
        if (IsRunning) return;
        if (dialogue == null || dialogue.LineCount == 0) return;

        active = dialogue;
        lineIndex = -1;

        if (ui != null) ui.Show();
        ShowNextLine();
    }

    private void TryAdvance()
    {
        if (advanceLockedUntilDelay && Time.time < advanceUnlockTime) return;

        if (!revealComplete)
        {
            // First press: finish the current bubble immediately.
            CompleteCurrentReveal();
        }
        else
        {
            // Second press (or press after auto-delay): move on.
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        lineIndex++;

        if (active == null || lineIndex >= active.LineCount)
        {
            EndConversation();
            return;
        }

        DialogueLine line = active.Lines[lineIndex];

        // Resolve speaker (fall back to most recent non-null speaker for convenience).
        NPCSpeakerSO speaker = line.speaker;
        if (speaker == null)
        {
            for (int i = lineIndex - 1; i >= 0; i--)
            {
                if (active.Lines[i].speaker != null)
                {
                    speaker = active.Lines[i].speaker;
                    break;
                }
            }
        }

        if (ui != null)
        {
            ui.SetSpeaker(speaker);
            ui.SetContinueIndicatorVisible(false);
        }

        revealComplete = false;
        advanceLockedUntilDelay = line.autoAdvanceDelay > 0f;
        advanceUnlockTime = Time.time + line.autoAdvanceDelay;

        if (revealRoutine != null) StopCoroutine(revealRoutine);
        revealRoutine = StartCoroutine(RevealLine(line, speaker));
    }

    private IEnumerator RevealLine(DialogueLine line, NPCSpeakerSO speaker)
    {
        string text = line.text ?? string.Empty;

        if (ui != null) ui.SetLine(string.Empty);

        if (charactersPerSecond <= 0f || text.Length == 0)
        {
            if (ui != null) ui.SetLine(text);
            FinishReveal();
            yield break;
        }

        float secondsPerChar = 1f / charactersPerSecond;
        var sb = new System.Text.StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            sb.Append(text[i]);
            if (ui != null) ui.SetLine(sb.ToString());

            if (speaker != null && speaker.TypingBlip != null && !char.IsWhiteSpace(text[i]))
            {
                ui?.PlayBlip(speaker.TypingBlip, speaker.TypingBlipVolume);
            }

            yield return new WaitForSeconds(secondsPerChar);
        }

        FinishReveal();
    }

    private void CompleteCurrentReveal()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        if (active != null && lineIndex >= 0 && lineIndex < active.LineCount)
        {
            string text = active.Lines[lineIndex].text ?? string.Empty;
            if (ui != null) ui.SetLine(text);
        }

        FinishReveal();
    }

    private void FinishReveal()
    {
        revealComplete = true;
        revealRoutine = null;
        if (ui != null) ui.SetContinueIndicatorVisible(true);
    }

    private void EndConversation()
    {
        active = null;
        lineIndex = -1;
        revealComplete = false;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        if (ui != null) ui.Hide();
        if (endedChannel != null) endedChannel.Raise();
    }
}
