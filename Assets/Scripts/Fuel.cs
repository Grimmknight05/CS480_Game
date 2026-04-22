using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Fuel : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject pickupVFXPrefab;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        GameManager gm = GameManager.Instance;
        if (gm != null) gm.CollectFuel();

        if (pickupVFXPrefab != null)
            Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
