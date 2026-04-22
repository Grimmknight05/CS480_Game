using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int fuelTarget = 4;
    [SerializeField] private int fuelCollected = 0;

    public event Action<int, int> FuelChanged;

    public int FuelCollected => fuelCollected;
    public int FuelTarget => fuelTarget;
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

    void Start()
    {
        FuelChanged?.Invoke(fuelCollected, fuelTarget);
    }

    public void CollectFuel()
    {
        fuelCollected = Mathf.Min(fuelCollected + 1, fuelTarget);
        Debug.Log($"Fuel: {fuelCollected}/{fuelTarget}");
        FuelChanged?.Invoke(fuelCollected, fuelTarget);
    }
}
