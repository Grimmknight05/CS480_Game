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

        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.IsFullyFueled) return;

        IcePlayerController player = other.GetComponentInParent<IcePlayerController>();
        if (player == null) return;

        player.SetFrictionBoots(true);
        unlocked = true;
        Debug.Log("Friction Boots UNLOCKED — ship fully fueled.");
    }
}
