using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/AudioZoneChannel")]
public class AudioZoneChannelSO : ScriptableObject
{
    // Now passes the zone itself, not just a bool
    public UnityAction<AudioZone, bool> OnZoneEvent; // zone, isEnter

    public void RaiseEvent(AudioZone zone, bool isEnter)
    {
        OnZoneEvent?.Invoke(zone, isEnter);
    }
}