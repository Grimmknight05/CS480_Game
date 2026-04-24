using UnityEngine;

// Author: Joshua Henrikson, GitHub Copilot (April 2026)


// Generic holder for any activator's current state.
public struct ActivatorState
{
    public string ActivatorID { get; set; }
    public object State { get; set; }
    public float Time { get; set; }

    public ActivatorState(string id, object state)
    {
        ActivatorID = id;
        State = state;
        Time = UnityEngine.Time.time;
    }
}
