using UnityEngine;
public class ZoneResetter : MonoBehaviour
{
    [SerializeField] private string myZoneId;
    [SerializeField] private LevelResetChannelSO resetChannel;

    void OnEnable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised += ResetZone;
    }

    void ResetZone()
    {
        PlayerSessionData session = FindAnyObjectByType<LevelManager>()?.Session;
        if (session != null && session.GetLastZone() == myZoneId)
        {
            // Reset all enemies, puzzles, etc. in this zone
            // e.g., re-enable enemies, reset puzzle states
        }
    }
}