using UnityEngine;

public class SimpleBoss : Boss
{
    [Header("Arena")]
    [SerializeField] private DoorLerp entranceDoor;
    [SerializeField] private DoorLerp exitDoor;

    [Header("Enemy Spawn")]
    [SerializeField] private GameObject enemySpawnPrefab;

    [Header("Environmental Hazards")]
    //[SerializeField] private MeteorShower meteorShower;
    [SerializeField] private bool enableMeteors = true;

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

           // if (enableMeteors && meteorShower != null)
           //     meteorShower.StartMeteorShower();
        }
    }

    protected override void DefeatBoss()
    {
        //if (meteorShower != null)
        //    meteorShower.StopMeteorShower();
        exitDoor?.Close();
        base.DefeatBoss();
        
    }
    protected override void ResetInternal()
    {
        base.ResetInternal();
        
        //if (meteorShower != null)
         //   meteorShower.StopMeteorShower();
            
        encounterStarted = false;
    }
}