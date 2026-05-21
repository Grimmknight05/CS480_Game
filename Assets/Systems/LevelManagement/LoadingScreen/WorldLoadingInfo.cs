using UnityEngine;
[CreateAssetMenu(menuName = "Loading/WorldInfo")]
public class WorldLoadingInfo : ScriptableObject
{
    public WorldSO world;            // reference to the target world
    public GameObject planetPrefab;  // the planet model to show in loading screen
    public TipListSO tipList;
    [Header("Transform Overrides (in Planet Container space)")]
    public Vector3 planetLocalPosition = Vector3.zero;
    //public Vector3 planetLocalRotation = Vector3.zero; // Euler angles
    //public Vector3 planetLocalScale = Vector3.one;
}