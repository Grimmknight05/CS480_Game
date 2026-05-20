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
        // Reload current level but keep the checkpoint (so player respawns at last saved point)
        if (LevelManager.Instance != null && LevelManager.Instance.TryGetCurrentWorld(out WorldSO currentWorld))
        {
            LevelManager.Instance.LoadWorld(currentWorld, true);  // keep checkpoint
        }
        else
        {
            playerHealth?.RestoreFull();
            resetChannel?.Raise();
        }
    }
    

    public void RequestContinue()
    {
        playerHealth?.RestoreFull();
        resetChannel?.Raise();
    }
}
