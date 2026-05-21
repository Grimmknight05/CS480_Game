// AreaSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Area", fileName = "NewArea")]
public class AreaSO : ScriptableObject
{
    [SerializeField] private string areaId;
    [SerializeField] private string displayName;

    public string AreaId => areaId;
    public string DisplayName => displayName;

    // Optional: you could store a default spawn point (as a prefab) here
    // [SerializeField] private GameObject defaultSpawnPrefab;
}