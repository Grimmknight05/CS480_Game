using UnityEngine;
[AddComponentMenu("Custom/Complete Area On Event")]
public class CompleteAreaOnEvent : MonoBehaviour
{
    [SerializeField] private AreaSO area;

    public void CompleteThisArea()
    {
        if (area == null) return;
        GameProgress.CompleteArea(area);
        GameProgress.ClearCheckpoint();  // Always clear checkpoint when area is completed
        Debug.Log($"Area '{area.AreaId}' completed via {name}.");
    }

    private bool checkpointBelongsToArea(Vector3 checkpointPos, AreaSO area)
    {
        GameProgress.ClearCheckpoint();
        return true;
    }
}