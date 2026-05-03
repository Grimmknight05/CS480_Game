using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
