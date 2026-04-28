using UnityEngine;
using System;

// Author: David Haddad - Created new ActivatorStateChannel to replace the old walkie-talkie system (4/25/26)
[CreateAssetMenu(fileName = "NewActivatorStateChannel", menuName = "Events/Activator State Channel")]
public class ActivatorStateChannel : ScriptableObject
{
    // The event that listeners will subscribe to
    public event Action<string, object> OnStateChanged;

    // The method the stones/levers will call to broadcast their state
    public void RaiseEvent(string activatorID, object state)
    {
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(activatorID, state);
        }
        else
        {
            Debug.LogWarning($"[ActivatorStateChannel] {activatorID} raised an event, but nobody was listening.");
        }
    }
}