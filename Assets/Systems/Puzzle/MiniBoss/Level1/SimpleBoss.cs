using UnityEngine;

public class SimpleBoss : Boss
{
    [Header("Arena")]
    [SerializeField] private DoorLerp entranceDoor;
    [SerializeField] private DoorLerp exitDoor;

    [Header("Enemy Spawn")]
    [SerializeField] private GameObject enemySpawnPrefab;

    private bool encounterStarted = false;

    protected override void Start()
    {
        base.enemyPrefab = enemySpawnPrefab;
        base.Start();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!encounterStarted && other.CompareTag("Player"))
        {
            encounterStarted = true;
            entranceDoor?.Open();
            BeginFight();
        }
    }

    protected override void DefeatBoss()
    {
        exitDoor?.Close();
        base.DefeatBoss();
    }
}