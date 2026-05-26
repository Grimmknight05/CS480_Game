using UnityEngine;

[CreateAssetMenu(menuName = "Game/Spawner", fileName = "NewSpawner")]
public class SpawnerSO : ScriptableObject
{
    // Just an identifier – no runtime data needed.
    [Tooltip("Used to look up the correct spawner in the scene.")]
    public string description;
}