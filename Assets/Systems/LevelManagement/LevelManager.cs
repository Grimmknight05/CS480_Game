using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelMetadata hubMetadata; // Hub scene metadata
    [SerializeField] private LevelEndEventChannelSO levelEndChannel;
    [SerializeField] private PlayerSessionData playerSessionData; // from existing code

    public LevelMetadata CurrentLevel { get; private set; }

    void OnEnable()
    {
        if (levelEndChannel != null)
            levelEndChannel.OnLevelEnd += OnLevelEnd;
    }

    void OnDisable()
    {
        if (levelEndChannel != null)
            levelEndChannel.OnLevelEnd -= OnLevelEnd;
    }

    public void LoadLevel(LevelMetadata level)
    {
        if (level == null) return;
        StartCoroutine(LoadSceneAsync(level.sceneName, () =>
        {
            CurrentLevel = level;
            // Notify level controller (if any) that level started
            var controller = FindObjectOfType<BaseLevelController>();
            controller?.OnLevelEnter();
        }));
    }

    public void ReturnToHub()
    {
        if (hubMetadata == null) return;
        StartCoroutine(LoadSceneAsync(hubMetadata.sceneName, () =>
        {
            CurrentLevel = null;
            // Clear checkpoint when leaving a level (optional – depends on design)
            playerSessionData?.ClearCheckpoint();
        }));
    }

    private IEnumerator LoadSceneAsync(string sceneName, System.Action onComplete = null)
    {
        // Optional: fade/transition
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
            yield return null;

        async.allowSceneActivation = true;
        yield return new WaitForSeconds(0.1f); // brief delay for scene activation
        onComplete?.Invoke();
    }

    private void OnLevelEnd(LevelMetadata level, bool success)
    {
        // Called after level is completed (e.g. reached goal)
        // Return to hub automatically or show UI
        if (success)
        {
            ReturnToHub();
        }
        // else maybe respawn inside same level – depends on death system
    }
}