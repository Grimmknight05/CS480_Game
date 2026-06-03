using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShipTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    private bool unlocked;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (unlocked) return;
        if (!other.CompareTag(playerTag)) return;

        FuelSessionData fuel = FuelSessionData.Instance;
        if (fuel == null || !fuel.IsFullyFueled) return;

        IcePlayerController player = other.GetComponentInParent<IcePlayerController>();
        if (player == null) return;

        player.SetFrictionBoots(true);
        unlocked = true;
        Debug.Log("Friction Boots UNLOCKED — ship fully fueled.");
    }
}
