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
    [SerializeField] private bool logHistoryChannelToConsole = true;

    [Header("Progress reset")]
    [Tooltip("Played when this tracker clears progress (Clear(), or wrong note if auto-reset is on).")]
    [SerializeField] private AudioClip progressResetClip;
    [SerializeField] [Range(0f, 1f)] private float resetSoundVolume = 1f;
    [Tooltip("Copy the melody order from your Musical Sequence Configuration. When enabled, a tap that cannot extend any prefix of this sequence clears progress.")]
    [SerializeField] private MushroomColor[] expectedSequenceForAutoReset;
    [SerializeField] private bool resetWhenSequenceBreaksExpectedPrefix;

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

        if (resetWhenSequenceBreaksExpectedPrefix &&
            expectedSequenceForAutoReset != null &&
            expectedSequenceForAutoReset.Length > 0 &&
            !TailMatchesSomePrefixOfExpected(history, expectedSequenceForAutoReset))
        {
            ResetProgressInternal(playSound: true);
            return;
        }

        RaiseSnapshotToChannel(history.ToArray());
    }

    /// <summary>Clears stored colors, notifies the puzzle channel, and plays <see cref="progressResetClip"/> if assigned.</summary>
    public void Clear()
    {
        ResetProgressInternal(playSound: history.Count > 0);
    }

    private void ResetProgressInternal(bool playSound)
    {
        bool hadEntries = history.Count > 0;
        history.Clear();
        RaiseSnapshotToChannel(System.Array.Empty<MushroomColor>());
        if (playSound && hadEntries)
            PlayProgressResetSound();
    }

    private void RaiseSnapshotToChannel(MushroomColor[] snapshot)
    {
        if (puzzleChannel == null) return;

        puzzleChannel.RaiseEvent(sequenceID, snapshot);
        if (logHistoryChannelToConsole)
        {
            Debug.Log($"[MushroomSequenceTracker] puzzle channel '{sequenceID}' history ({snapshot.Length}): {string.Join(", ", snapshot)}");
        }
    }

    private void PlayProgressResetSound()
    {
        if (progressResetClip == null) return;
        AudioSource.PlayClipAtPoint(progressResetClip, transform.position, resetSoundVolume);
    }

    /// <summary>
    /// True when the tail of <paramref name="hist"/> matches expected[0..k-1] for some k (progress toward the target melody).
    /// </summary>
    private static bool TailMatchesSomePrefixOfExpected(List<MushroomColor> hist, MushroomColor[] expected)
    {
        int n = hist.Count;
        if (n == 0) return true;

        int maxK = Mathf.Min(n, expected.Length);
        for (int k = maxK; k >= 1; k--)
        {
            bool ok = true;
            for (int i = 0; i < k; i++)
            {
                if (hist[n - k + i] != expected[i])
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return true;
        }
        return false;
    }
}
