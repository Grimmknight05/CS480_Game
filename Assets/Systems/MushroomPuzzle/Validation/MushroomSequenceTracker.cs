using System.Collections.Generic;
using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Bridges per-mushroom activations to the existing puzzle framework. Subscribes
// to MushroomEventChannelSO, accumulates a bounded list of recent colors, and
// re-raises the running sequence on ActivatorStateChannel so PuzzleValidator can
// match a MusicalSequenceConfiguration with the same sequenceID.

public class MushroomSequenceTracker : MonoBehaviour
{
    [SerializeField] private MushroomEventChannelSO mushroomChannel;
    [SerializeField] private ActivatorStateChannel puzzleChannel;
    [SerializeField] private string sequenceID = "MushroomSequence";
    [SerializeField] private int maxHistory = 20;

    private readonly List<MushroomColor> history = new List<MushroomColor>();

    private void OnEnable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised += OnMushroomActivated;
    }

    private void OnDisable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised -= OnMushroomActivated;
    }

    private void OnMushroomActivated(MushroomActivationData data)
    {
        history.Add(data.Color);
        if (history.Count > maxHistory) history.RemoveAt(0);

        if (puzzleChannel != null)
        {
            puzzleChannel.RaiseEvent(sequenceID, history.ToArray());
        }
    }

    public void Clear()
    {
        history.Clear();
    }
}
