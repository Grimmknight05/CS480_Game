using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private LevelResetChannelSO resetChannel;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverUI; // assign in Inspector

    private bool isDead = false;

    private void OnEnable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (isDead) return;
        isDead = true;
        
        // Show Game Over UI, pause game if needed
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        
        // Optionally lock cursor or stop player input
        CursorHelper.Unlock(); // show cursor for UI
        Time.timeScale = 0f; // pause game (if you want full pause)
    }

    public void RequestContinue()
    {
        if (!isDead) return;
        
        isDead = false;
        
        // Hide UI
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        
        // Resume time
        Time.timeScale = 1f;
        
        // Lock cursor again
        CursorHelper.Lock();
        
        // Trigger respawn
        resetChannel?.Raise();
        
        // Also restore health directly if needed (soft respawn may already do that)
        playerHealth?.RestoreFull();
    }
}