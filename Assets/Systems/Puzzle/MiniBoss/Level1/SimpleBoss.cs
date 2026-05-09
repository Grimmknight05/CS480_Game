using UnityEngine;

public class SimpleBoss : Boss
{
    [Header("Arena")]
    [SerializeField] private DoorLerp entranceDoor;
    [SerializeField] private DoorLerp exitDoor;
    [SerializeField] private BossPillar[] pillars;
    private GameObject enemySpawnPrefab;

    private bool encounterStarted = false;

    protected override void Start()
    {
        base.enemyPrefab = enemySpawnPrefab; // assign to base field
        base.Start();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!encounterStarted && other.CompareTag("Player"))
        {
            StartBossEncounter();
        }
    }

    private void StartBossEncounter()
    {
        encounterStarted = true;
        entranceDoor?.Close();
        foreach (var p in pillars) p.MaxHight();
    }

    protected override void DefeatBoss()
    {
        exitDoor?.Open();
        foreach (var p in pillars) p.Reset();
        base.DefeatBoss();
    }
}