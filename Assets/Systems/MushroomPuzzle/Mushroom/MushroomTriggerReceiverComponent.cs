using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Component pattern: detects the player entering the mushroom's trigger collider
// and queues an InteractCommand on the Mushroom orchestrator.

[RequireComponent(typeof(Collider))]
public class MushroomTriggerReceiverComponent : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Mushroom mushroom;

    private void Awake()
    {
        if (mushroom == null) mushroom = GetComponentInParent<Mushroom>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mushroom == null) return;
        if (!other.CompareTag(playerTag)) return;
        if (mushroom.IsInteractionLocked) return;
        mushroom.QueueCommand(new InteractCommand());
    }
}
