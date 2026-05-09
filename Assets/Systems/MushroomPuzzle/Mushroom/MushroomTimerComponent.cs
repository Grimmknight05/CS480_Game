using System;
using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Update Method pattern: ticks one frame at a time using Time.deltaTime instead
// of a coroutine, so it pauses naturally with the editor and is trivially testable.

public class MushroomTimerComponent : MonoBehaviour
{
    public event Action OnExpired;

    [SerializeField] private float remaining;
    [SerializeField] private bool running;

    public bool IsRunning => running;
    public float Remaining => remaining;

    public void StartTimer(float seconds)
    {
        remaining = Mathf.Max(0f, seconds);
        running = remaining > 0f;
    }

    public void Cancel()
    {
        running = false;
        remaining = 0f;
    }

    private void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        if (remaining > 0f) return;

        running = false;
        remaining = 0f;
        OnExpired?.Invoke();
    }
}
