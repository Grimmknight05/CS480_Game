using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Zone")]
    public BoxCollider spawnArea;
    public float meteorStartHeight = 8f;

    [Header("Timing")]
    public float warningDuration = 1.5f;
    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 3f;

    [Header("Prefabs")]
    public Meteor meteorPrefab;
    public Indicator indicatorPrefab;

    [Header("Pool Sizes")]
    public int initialMeteorPoolSize = 10;
    public int initialIndicatorPoolSize = 5;

    private ObjectPool<Meteor> meteorPool;
    private ObjectPool<Indicator> indicatorPool;

    void Awake()
    {
        meteorPool = new ObjectPool<Meteor>(meteorPrefab, initialMeteorPoolSize);
        indicatorPool = new ObjectPool<Indicator>(indicatorPrefab, initialIndicatorPoolSize);
    }

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
            StartCoroutine(SpawnMeteorSequence());
        }
    }

    IEnumerator SpawnMeteorSequence()
    {
        // 1. Random ground position inside the box collider
        Vector3 groundPos = GetRandomPointInBox(spawnArea);
        groundPos.y = 0;   // assume ground Y = 0

        // 2. Get indicator from pool and initialize it
        Indicator indicator = indicatorPool.Get();
        indicator.Initialize(indicatorPool, groundPos);

        // 3. Wait, then spawn meteor above the same spot
        yield return new WaitForSeconds(warningDuration);

        Vector3 meteorPos = new Vector3(groundPos.x, meteorStartHeight, groundPos.z);
        Meteor meteor = meteorPool.Get();
        meteor.Initialize(meteorPool, meteorPos);
    }

    Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.center;
        Vector3 size = box.size;
        float x = Random.Range(center.x - size.x/2, center.x + size.x/2);
        float z = Random.Range(center.z - size.z/2, center.z + size.z/2);
        return new Vector3(x, 0, z);
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }
    }
}