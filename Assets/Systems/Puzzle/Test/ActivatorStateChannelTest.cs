using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle/Event Channels/Activator State Channel")]
public class ActivatorStateChannelTest : ScriptableObject
{
    public event Action<ActivatorID, object> OnStateChanged;

    public void Raise(ActivatorID id, object state)
    {
        OnStateChanged?.Invoke(id, state);
    }
}