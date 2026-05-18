using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [SerializeField] private LevelEndEventChannelSO levelEndChannel;
    [SerializeField] private LevelMetadata[] allLevels;

    private Dictionary<string, bool> completedLevels = new Dictionary<string, bool>();

    void OnEnable()
    {
        if (levelEndChannel != null)
            levelEndChannel.OnLevelEnd += HandleLevelEnd;
        LoadProgress();
    }

    void OnDisable()
    {
        if (levelEndChannel != null)
            levelEndChannel.OnLevelEnd -= HandleLevelEnd;
    }

    void HandleLevelEnd(LevelMetadata level, bool success)
    {
        if (success && !IsLevelCompleted(level))
        {
            completedLevels[level.sceneName] = true;
            SaveProgress();
        }
    }

    public bool IsLevelCompleted(LevelMetadata level)
    {
        return completedLevels.ContainsKey(level.sceneName) && completedLevels[level.sceneName];
    }

    public bool IsLevelUnlocked(LevelMetadata level)
    {
        // Example logic: first level always unlocked, others require previous completion
        return level.levelIndex == 0 || IsLevelCompleted(GetPreviousLevel(level));
    }

    private LevelMetadata GetPreviousLevel(LevelMetadata level)
    {
        // Simple implementation: previous level in build order
        // Could be more sophisticated using planetID + levelIndex
        return allLevels.Length > level.levelIndex - 1 ? allLevels[level.levelIndex - 1] : null;
    }

    private void LoadProgress()
    {
        // Use PlayerPrefs or JSON file
        foreach (var level in allLevels)
        {
            bool completed = PlayerPrefs.GetInt($"Completed_{level.sceneName}", 0) == 1;
            if (completed) completedLevels[level.sceneName] = true;
        }
    }

    private void SaveProgress()
    {
        foreach (var kvp in completedLevels)
        {
            PlayerPrefs.SetInt($"Completed_{kvp.Key}", kvp.Value ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
}