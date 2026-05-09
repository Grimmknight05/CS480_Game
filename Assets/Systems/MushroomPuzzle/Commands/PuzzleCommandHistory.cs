using System.Collections.Generic;
using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Bounded log of executed puzzle commands. Each entry pairs a command with the
// mushroom that received it so UndoLast can call cmd.Undo on the right target.
// Useful for sequence inspection and a debug-key Undo action.

public class PuzzleCommandHistory : MonoBehaviour
{
    [SerializeField] private int capacity = 64;

    private readonly List<Entry> entries = new List<Entry>();

    public IReadOnlyList<Entry> Entries => entries;
    public int Count => entries.Count;

    public void RecordExecuted(IPuzzleCommand command, Mushroom receiver)
    {
        if (command == null || receiver == null) return;
        entries.Add(new Entry(command, receiver, Time.time));
        if (entries.Count > capacity) entries.RemoveAt(0);
    }

    public void UndoLast()
    {
        if (entries.Count == 0) return;
        var last = entries[entries.Count - 1];
        entries.RemoveAt(entries.Count - 1);
        last.Command.Undo(last.Receiver);
    }

    public readonly struct Entry
    {
        public readonly IPuzzleCommand Command;
        public readonly Mushroom Receiver;
        public readonly float ExecutedAtTime;

        public Entry(IPuzzleCommand command, Mushroom receiver, float executedAtTime)
        {
            Command = command;
            Receiver = receiver;
            ExecutedAtTime = executedAtTime;
        }
    }
}
