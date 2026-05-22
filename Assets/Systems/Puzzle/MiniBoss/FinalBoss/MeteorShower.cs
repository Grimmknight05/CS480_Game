using UnityEngine;
using System.Collections.Generic;

public class MeteorShower : MonoBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float meteorSpeed = 20f;
    [SerializeField] private float meteorDamage = 25f;
    [SerializeField] private float warningTime = 1.5f;
    
    [Header("Spawning")]
    [SerializeField] private float spawnIntervalMin = 3f;
    [SerializeField] private float spawnIntervalMax = 8f;
    [SerializeField] private int meteorsPerWave = 3;
    
    [Header("Visuals")]
    [SerializeField] private GameObject warningIndicatorPrefab;
    
    private bool isActive = false;
    private Coroutine meteorCoroutine;
    
    public void StartMeteorShower()
    {
        if (isActive) return;
        isActive = true;
        meteorCoroutine = StartCoroutine(SpawnMeteors());
    }
    
    public void StopMeteorShower()
    {
        isActive = false;
        if (meteorCoroutine != null)
            StopCoroutine(meteorCoroutine);
    }
    
    private System.Collections.IEnumerator SpawnMeteors()
    {
        while (isActive)
        {
            float waitTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(waitTime);
            
            // Spawn multiple meteors
            for (int i = 0; i < meteorsPerWave; i++)
            {
                SpawnSingleMeteor();
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
    
    private void SpawnSingleMeteor()
    {
        if (spawnPoints.Length == 0) return;
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 targetPosition = GetRandomGroundPosition();
        
        // Show warning indicator
        if (warningIndicatorPrefab != null)
        {
            var warning = Instantiate(warningIndicatorPrefab, targetPosition, Quaternion.identity);
            Destroy(warning, warningTime);
        }
        
        // Spawn meteor after warning
        StartCoroutine(SpawnMeteorWithDelay(spawnPoint.position, targetPosition));
    }
    
    private System.Collections.IEnumerator SpawnMeteorWithDelay(Vector3 startPos, Vector3 targetPos)
    {
        yield return new WaitForSeconds(warningTime);
        
        GameObject meteor = Instantiate(meteorPrefab, startPos, Quaternion.identity);
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        
        if (meteorScript == null)
        {
            // Simple movement if no script
            meteorScript = meteor.AddComponent<Meteor>();
        }
        
        meteorScript.Initialize(targetPos, meteorSpeed, meteorDamage);
    }
    
    private Vector3 GetRandomGroundPosition()
    {
        // Adjust based on your arena bounds
        float arenaRadius = 20f;
        Vector2 randomCircle = Random.insideUnitCircle * arenaRadius;
        return new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}