using UnityEngine;

public abstract class BaseLevelController : MonoBehaviour
{
    [SerializeField] protected LevelMetadata levelMetadata;
    [SerializeField] protected LevelEndEventChannelSO levelEndChannel;

    // Called by the level (e.g. when player reaches goal)
    public void LevelEnd(bool success)
    {
        levelEndChannel?.Raise(levelMetadata, success);
        // Optionally perform level‑specific cleanup (sandbox method)
        OnLevelCompleted(success);
    }

    // Sandbox methods – override in concrete level scripts
    protected virtual void OnLevelCompleted(bool success) { }
    public virtual void OnLevelEnter() { }
    public virtual void OnLevelExit() { }
}