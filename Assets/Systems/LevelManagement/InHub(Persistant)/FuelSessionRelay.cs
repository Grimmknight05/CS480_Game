using UnityEngine;

// Persistent bridge between the decoupled fuel pickup event and the fuel store/UI.
//
// It is a guarded DontDestroyOnLoad bootstrapper (NOT a holder of game state — the
// transient count lives in the FuelSessionData SO, so this stays within CLAUDE.md's
// Singleton Constraints). Place one copy in EVERY playable scene (hub + each level):
//   - Boot from the hub: the hub's relay persists; a level's own relay sees an existing
//     instance and destroys itself.
//   - Open a level directly (editor testing / standalone load): that scene's relay
//     initializes on its own, so fuel collection still works without the hub/LevelManager.
//
// Its three hard [SerializeField] references pin FuelSessionData and both channels
// against WebGL's Resources.UnloadUnusedAssets() across scene loads.
//
// Pipeline (Observer pattern, no cross-domain coupling):
//   Fuel.OnTriggerEnter -> FuelCollectedChannel.Raise()
//        -> FuelSessionRelay.HandleCollected() -> FuelSessionData.AddFuel()
//        -> FuelStateChannel.Raise(FuelState) -> FuelTerminalUI / any listener
public class FuelSessionRelay : MonoBehaviour
{
    public static FuelSessionRelay Instance { get; private set; }

    [Header("Persistent Fuel Store (hard pin for WebGL)")]
    [SerializeField] private FuelSessionData fuelSession;

    [Header("Event Channels")]
    [SerializeField] private FuelCollectedChannel fuelCollectedChannel;
    [SerializeField] private FuelStateChannel fuelStateChannel;

    private bool isActiveInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A relay already survives from an earlier scene — this duplicate stands down.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        isActiveInstance = true;

        if (fuelSession != null)
            FuelSessionData.SetInstance(fuelSession);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (!isActiveInstance) return; // duplicate awaiting destruction — do not subscribe
        if (fuelCollectedChannel != null)
            fuelCollectedChannel.OnRaised += HandleCollected;
    }

    private void OnDisable()
    {
        if (!isActiveInstance) return;
        if (fuelCollectedChannel != null)
            fuelCollectedChannel.OnRaised -= HandleCollected;
    }

    private void Start()
    {
        if (!isActiveInstance) return;
        // Broadcast the current state once so freshly-loaded UIs sync immediately.
        BroadcastState();
    }

    private void HandleCollected()
    {
        if (fuelSession == null) return;

        fuelSession.AddFuel();
        Debug.Log($"Fuel: {fuelSession.Collected}/{fuelSession.Target}");
        BroadcastState();
    }

    private void BroadcastState()
    {
        if (fuelSession == null || fuelStateChannel == null) return;
        fuelStateChannel.Raise(new FuelState(fuelSession.Collected, fuelSession.Target));
    }
}
