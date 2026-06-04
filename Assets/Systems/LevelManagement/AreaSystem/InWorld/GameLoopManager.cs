using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private LevelResetChannelSO resetChannel;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverUI; // assign in Inspector

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;

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

        // Play death SFX before timeScale hits 0 (audio is unaffected by timeScale).
        if (deathSound != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(deathSound, pos, 1f);
        }

        // Show Game Over UI, pause game if needed
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        CursorHelper.Unlock();
        Time.timeScale = 0f;
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