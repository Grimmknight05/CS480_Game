using System;
using UnityEngine;

public class StateEvent
{
    public string id;
    public bool state;
    public object source;

    public StateEvent(string id, bool state, object source)
    {
        this.id = id;
        this.state = state;
        this.source = source;
    }
}