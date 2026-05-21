using UnityEngine;
[AddComponentMenu("Custom/Complete Area On Event")]
public class CompleteAreaOnEvent : MonoBehaviour
{
    [SerializeField] private AreaSO area;

    public void CompleteThisArea()
    {
        if (area == null)
        {
            Debug.LogWarning($"{name}: No AreaSO assigned.", this);
            return;
        }
        GameProgress.CompleteArea(area);
        Debug.Log($"Area '{area.AreaId}' completed via {name}.");
    }
}