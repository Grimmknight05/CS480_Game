using UnityEngine;

public class MovementZone : MonoBehaviour
{
    [SerializeField] private MovementMode zoneMode;

    private void OnTriggerEnter(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponent<PlayerControllerRefactored>();

        if (player != null)
        {
            player.SetMovementMode(zoneMode);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponent<PlayerControllerRefactored>();

        if (player != null)
        {
            // Revert to default when leaving
            //player.SetMovementMode(player.GetDefaultMode());
        }
    }
}
