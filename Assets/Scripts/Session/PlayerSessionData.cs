using UnityEngine;

[CreateAssetMenu(menuName = "Session/Player Session Data", fileName = "PlayerSessionData")]
public class PlayerSessionData : ScriptableObject
{
    [SerializeField] private Vector3 lastCheckpoint;
    [SerializeField] private bool hasCheckpoint;

    void OnEnable()
    {
        // SO field mutations during Play Mode dirty the .asset file in the Editor;
        // resetting on load guarantees a clean session every Play-Mode entry.
        hasCheckpoint = false;
        lastCheckpoint = Vector3.zero;
    }

    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
        hasCheckpoint = true;
    }

    public void ClearCheckpoint()
    {
        lastCheckpoint = Vector3.zero;
        hasCheckpoint = false;
    }

    public bool TryGetCheckpoint(out Vector3 pos)
    {
        pos = lastCheckpoint;
        return hasCheckpoint;
    }
}
