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

    [Header("Group Identifier")]
    public MeteorSpawnerGroupSO spawnerGroup;

    private ObjectPool<Meteor> meteorPool;
    private ObjectPool<Indicator> indicatorPool;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        meteorPool = new ObjectPool<Meteor>(meteorPrefab, initialMeteorPoolSize);
        indicatorPool = new ObjectPool<Indicator>(indicatorPrefab, initialIndicatorPoolSize);
    }

    void OnEnable()
    {
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (enabled)  // check enabled flag each loop
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
            StartCoroutine(SpawnMeteorSequence());
        }
    }

    IEnumerator SpawnMeteorSequence()
    {
        // 1. Random ground position inside the box collider (XZ)
        Vector3 groundPos = GetRandomPointInBox(spawnArea);
        
        // 2. Raycast down to get real ground height
        float groundY = 0f;
        RaycastHit hit;
        if (Physics.Raycast(groundPos + Vector3.up * 100f, Vector3.down, out hit, 200f, ~0, QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
        }
        groundPos.y = groundY;

        // 3. Get indicator from pool and initialize it
        Indicator indicator = indicatorPool.Get();
        indicator.Initialize(indicatorPool, groundPos);

        // 4. Wait, then spawn meteor above the same spot
        yield return new WaitForSeconds(warningDuration);

        Vector3 meteorPos = new Vector3(groundPos.x, groundY + meteorStartHeight, groundPos.z);
        Meteor meteor = meteorPool.Get();
        meteor.Initialize(meteorPool, meteorPos);
    }

    Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 size = box.size;
        float x = Random.Range(-size.x/2, size.x/2);
        float z = Random.Range(-size.z/2, size.z/2);
        Vector3 localPoint = new Vector3(x, 0, z);
        return box.transform.TransformPoint(localPoint);
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea != null)
        {
            Gizmos.color = Color.red;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
            Gizmos.matrix = originalMatrix;
        }
    }
}