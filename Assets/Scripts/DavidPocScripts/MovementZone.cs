using System.Collections.Generic;
using UnityEngine;

public class MovementZone : MonoBehaviour
{
    [SerializeField] private MovementMode zoneMode;
    private static Dictionary<PlayerControllerRefactored, MovementMode> previousModes = new Dictionary<PlayerControllerRefactored, MovementMode>();

    private void OnTriggerEnter(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponent<PlayerControllerRefactored>();
        if (player != null)
        {
            if (!previousModes.ContainsKey(player))
                previousModes[player] = player.CurrentMovementMode; // you'd need a getter
            player.SetMovementMode(zoneMode);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponent<PlayerControllerRefactored>();
        if (player != null && previousModes.TryGetValue(player, out MovementMode previous))
        {
            player.SetMovementMode(previous);
            previousModes.Remove(player);
        }
    }
}