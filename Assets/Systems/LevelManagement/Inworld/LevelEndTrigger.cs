using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = true;
    [Tooltip("Scene name to load if LevelManager is not present (for direct testing)")]
    [SerializeField] private string fallbackHubScene = "J'sSpaceHub";
    [SerializeField] private bool isTriggerable = true;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if(!isTriggerable){return;}
            
        if (oneShot && triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;

        GameEnd();
    }
    private void GameEnd()
    {
        // Try to use LevelManager if it exists
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteCurrentLevel();
        }
        else
        {
            Debug.LogWarning("LevelManager not found – loading fallback hub scene directly.");
            GameProgress.ClearCheckpoint(); // optional cleanup
            SceneManager.LoadScene(fallbackHubScene);
        }
    }
    public void callEndGame()
    {
        GameEnd();
    }
}