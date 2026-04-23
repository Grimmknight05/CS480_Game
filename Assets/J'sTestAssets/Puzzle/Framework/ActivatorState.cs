/// <summary>
/// Generic holder for any activator's current state.
/// Works with rotations, button presses, lever positions, etc.
/// </summary>
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
