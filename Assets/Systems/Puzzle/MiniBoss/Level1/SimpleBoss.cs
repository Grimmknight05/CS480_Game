using UnityEngine;

public class SimpleBoss : Boss
{
    [Header("Arena Doors")]
    [SerializeField] private DoorLerp entranceDoor;
    [SerializeField] private DoorLerp exitDoor;

    [Header("Pillars")]
    [SerializeField] private BossPillar[] pillars;

    [Header("Phase List (assign in inspector)")]
    [SerializeField] private SimplePhase[] phaseList;   // create SimplePhase asset or use inspector array

    private bool encounterStarted = false;

    protected override void Start()
    {
        // Assign phases array from the list
        phases = phaseList;
        SetupPhases();
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
        foreach (var pillar in pillars)
            pillar.MaxHight();
        // Boss is already active; phases will run automatically
    }

    protected override void DefeatBoss()
    {
        exitDoor?.Open();
        base.DefeatBoss();
    }
}