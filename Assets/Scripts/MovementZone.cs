using UnityEngine;

public class MovementZone : MonoBehaviour
{
    [SerializeField] private MovementMode zoneMode;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.SetMovementMode(zoneMode);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // Revert to default when leaving
            player.SetMovementMode(player.GetDefaultMode());
        }
    }
}
