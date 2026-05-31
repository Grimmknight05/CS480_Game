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

    [Header("Progression")]
    [Tooltip("This world stays locked until every world listed here is completed.")]
    public WorldSO[] prerequisiteWorlds;

    // Returns true if all prerequisite worlds have been completed in the session.
    public bool IsUnlocked(PlayerSessionData session)
    {
        if (prerequisiteWorlds == null || prerequisiteWorlds.Length == 0)
            return true;
        if (session == null)
            return false;

        foreach (WorldSO prerequisite in prerequisiteWorlds)
        {
            if (prerequisite != null && !session.IsWorldCompleted(prerequisite))
                return false;
        }
        return true;
    }

    // Called when level is completed
    public virtual void OnLevelComplete(PlayerSessionData session)
    {
        // Record this world as completed so dependent worlds unlock.
        // Override in derived SOs for additional custom logic (call base first).
        session?.CompleteWorld(this);
    }

    // Called before loading this world (optional setup)
    public virtual void OnWillLoad(PlayerSessionData session) { }

    // Called after scene is loaded (post-initialization)
    public virtual void OnDidLoad(PlayerSessionData session) { }
}