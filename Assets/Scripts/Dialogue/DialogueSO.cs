// =====================================================================
// A conversation asset. Each DialogueSO holds a list of DialogueLine
// entries, where each line specifies who is speaking and what they
// say. Conversation-level flags (playOnce, retriggerCooldown) let
// the same trigger be used for one-off intros or repeatable ambient
// chatter without changing code.
// =====================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [Tooltip("Speaker for this line. If empty, the previous line's speaker is reused.")]
    public NPCSpeakerSO speaker;

    [TextArea(2, 8)]
    [Tooltip("The text shown in this single bubble.")]
    public string text;

    [Tooltip("Optional pause (in seconds) automatically inserted after this line " +
             "before the player can advance. 0 = no forced pause.")]
    [Min(0f)]
    public float autoAdvanceDelay = 0f;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue", fileName = "NewDialogue")]
public class DialogueSO : ScriptableObject
{
    [Header("Lines")]
    [Tooltip("The bubbles that play in order. Each entry is one bubble in the UI.")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Playback")]
    [Tooltip("If true, this conversation only plays the first time it is triggered.")]
    [SerializeField] private bool playOnce = false;

    [Tooltip("If > 0, the conversation cannot be retriggered until this many seconds " +
             "have passed since it last finished. Ignored if playOnce is true.")]
    [Min(0f)]
    [SerializeField] private float retriggerCooldown = 0f;

    public IReadOnlyList<DialogueLine> Lines => lines;
    public bool PlayOnce => playOnce;
    public float RetriggerCooldown => retriggerCooldown;

    public int LineCount => lines == null ? 0 : lines.Count;
}
