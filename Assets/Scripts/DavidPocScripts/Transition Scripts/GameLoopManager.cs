using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private LevelResetChannelSO resetChannel;
    [SerializeField] private PlayerHealth playerHealth;

    void OnEnable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised += HandlePlayerDeath;
    }

    void OnDisable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised -= HandlePlayerDeath;
    }

    void HandlePlayerDeath()
    {
        // The Game Over UI gates the soft reset now — see RequestContinue.
    }

    public void RequestContinue()
    {
        playerHealth?.RestoreFull();
        resetChannel?.Raise();
    }
}
