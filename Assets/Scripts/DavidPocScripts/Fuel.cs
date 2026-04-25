using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Fuel : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject pickupVFXPrefab;
    [SerializeField] private FuelCollectedChannel fuelCollectedChannel;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fuelCollectedChannel != null)
            fuelCollectedChannel.Raise();
        else
            Debug.LogWarning("Fuel: no FuelCollectedChannel assigned; pickup will not register.", this);

        if (pickupVFXPrefab != null)
            Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
