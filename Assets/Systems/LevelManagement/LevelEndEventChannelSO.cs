using UnityEngine;

[CreateAssetMenu(menuName = "Events/Level End Channel", fileName = "LevelEnd.channel.asset")]
public class LevelEndEventChannelSO : ScriptableObject
{
    public System.Action<LevelMetadata, bool> OnLevelEnd; // bool = success

    public void Raise(LevelMetadata level, bool success)
    {
        OnLevelEnd?.Invoke(level, success);
    }
}