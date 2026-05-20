// WorldSO.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Level/World", fileName = "NewWorld")]
public class WorldSO : ScriptableObject
{
    [Header("Scene Reference")]
    public string sceneName;          // Must match build settings
    public string displayName;
    public Sprite previewImage;

    [Header("Level Data")]
    public bool isHubWorld = false;    // Space station hub flag
    public int requiredProgressLevel;  // Minimum progress to unlock
    public AudioClip levelMusic;
    public bool overrideGravity;
    public Vector3 customGravity;
    
    [Header("Player Settings Override")]
    public MovementMode defaultMovementMode = MovementMode.AccelerationBased;
    public bool overrideMovementMode = false; // if false, keep prefab's default

    // Called when level is completed
    public virtual void OnLevelComplete(PlayerSessionData session)
    {
        // Override in derived SOs for custom logic
        // e.g., update session progress, unlock next level
        //session.SetCheckpoint(defaultSpawnPoint); // example
        //session.completedLevels.Add(this.sceneName);
    }

    // Called before loading this world (optional setup)
    public virtual void OnWillLoad(PlayerSessionData session) { }

    // Called after scene is loaded (post-initialization)
    public virtual void OnDidLoad(PlayerSessionData session) { }
}