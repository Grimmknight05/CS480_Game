using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointGroup;
    public string Group => spawnPointGroup;
    public Vector3 Position => transform.position;
}