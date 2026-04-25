using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int fuelTarget = 4;
    [SerializeField] private int fuelCollected = 0;

    [SerializeField] private FuelCollectedChannel fuelCollectedChannel;
    [SerializeField] private FuelStateChannel fuelStateChannel;

    public bool IsFullyFueled => fuelCollected >= fuelTarget;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        if (fuelCollectedChannel != null)
            fuelCollectedChannel.OnRaised += HandleFuelCollected;
    }

    void OnDisable()
    {
        if (fuelCollectedChannel != null)
            fuelCollectedChannel.OnRaised -= HandleFuelCollected;
    }

    void Start()
    {
        RaiseState();
    }

    void HandleFuelCollected()
    {
        fuelCollected = Mathf.Min(fuelCollected + 1, fuelTarget);
        Debug.Log($"Fuel: {fuelCollected}/{fuelTarget}");
        RaiseState();
    }

    void RaiseState()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.Raise((fuelCollected, fuelTarget));
    }
}
