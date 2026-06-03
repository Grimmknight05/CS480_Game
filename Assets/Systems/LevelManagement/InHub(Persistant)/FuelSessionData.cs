using UnityEngine;

// Persistent fuel store. Lives as a ScriptableObject asset so the collected count
// survives full scene loads in WebGL (where Resources.UnloadUnusedAssets() runs on
// SceneManager.LoadScene). A persistent owner — FuelSessionRelay on the LevelManager
// GameObject — holds a hard [SerializeField] reference so this SO is never GC'd, and
// registers the static Instance via SetInstance (mirrors PlayerSessionData).
[CreateAssetMenu(menuName = "Session/Fuel Session Data", fileName = "FuelSessionData")]
public class FuelSessionData : ScriptableObject
{
    [SerializeField] private int fuelTarget = 4;
    [SerializeField] private int fuelCollected = 0;

    private static FuelSessionData _instance;
    public static void SetInstance(FuelSessionData data) => _instance = data;
    public static FuelSessionData Instance => _instance;

    public int Collected => fuelCollected;
    public int Target => fuelTarget;
    public bool IsFullyFueled => fuelCollected >= fuelTarget;

    // Editor Persistence Protocol: SOs mutate permanently in Play Mode, so wipe the
    // transient runtime count on load to guarantee a clean session start. fuelTarget
    // is configuration, not transient state, so it is left untouched.
    private void OnEnable()
    {
        fuelCollected = 0;
    }

    public void AddFuel(int amount = 1)
    {
        fuelCollected = Mathf.Min(fuelCollected + amount, fuelTarget);
    }

    public void ResetFuel() => fuelCollected = 0;
}
