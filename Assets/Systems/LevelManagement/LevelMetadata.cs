using UnityEngine;

[CreateAssetMenu(menuName = "Level/Level Metadata", fileName = "LevelMetadata_")]
public class LevelMetadata : ScriptableObject
{
    public string sceneName;      // Must match scene build index name
    public string displayName;
    public int planetID;          // For grouping into planets
    public int levelIndex;        // Order within planet
    public Sprite previewImage;
    public bool isUnlocked;       // This can be overridden by save data
}