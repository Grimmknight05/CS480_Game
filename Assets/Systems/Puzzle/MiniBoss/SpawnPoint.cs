using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private SpawnSO spawnSO;   // replaces spawnPointGroup string
    public SpawnSO SpawnSO => spawnSO;
    public Vector3 Position => transform.position;
}