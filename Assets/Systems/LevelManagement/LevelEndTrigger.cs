// LevelEndTrigger.cs
using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private WorldSO thisWorld; // optional, for validation

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.Instance.CompleteCurrentLevel();
        }
    }

    // Alternative: call from UI button
    public void EndLevelManually() => LevelManager.Instance.CompleteCurrentLevel();
}