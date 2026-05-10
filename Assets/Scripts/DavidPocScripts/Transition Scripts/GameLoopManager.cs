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
        // Soft reset: fan out to all subscribers (pushables, enemies, puzzles, spawn positioner)
        // instead of reloading the scene, so that permanently-solved puzzles can opt out.
        resetChannel?.Raise();
        playerHealth?.RestoreFull();
    }
}
