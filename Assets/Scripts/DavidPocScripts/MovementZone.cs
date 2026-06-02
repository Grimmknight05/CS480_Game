using System.Collections.Generic;
using UnityEngine;

public class MovementZone : MonoBehaviour
{
    [SerializeField] private MovementMode zoneMode;
    private static readonly Dictionary<PlayerControllerRefactored, ZoneEntry> previousModes = new Dictionary<PlayerControllerRefactored, ZoneEntry>();

    private struct ZoneEntry
    {
        public MovementMode PreviousMode;
        public MovementMode ActiveMode;
        public int OverlapCount;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponentInParent<PlayerControllerRefactored>();
        if (player == null)
            return;

        if (previousModes.TryGetValue(player, out ZoneEntry entry))
        {
            entry.OverlapCount++;
            previousModes[player] = entry;
            return;
        }

        previousModes[player] = new ZoneEntry
        {
            PreviousMode = player.CurrentMovementMode,
            ActiveMode = zoneMode,
            OverlapCount = 1
        };

        player.SetMovementMode(zoneMode);

        if (zoneMode == MovementMode.ZeroGrav)
            ZeroGInstructionHUD.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerControllerRefactored player = other.GetComponentInParent<PlayerControllerRefactored>();
        if (player == null || !previousModes.TryGetValue(player, out ZoneEntry entry))
            return;

        entry.OverlapCount--;
        if (entry.OverlapCount > 0)
        {
            previousModes[player] = entry;
            return;
        }

        player.SetMovementMode(entry.PreviousMode);
        previousModes.Remove(player);

        if (entry.ActiveMode == MovementMode.ZeroGrav)
            ZeroGInstructionHUD.Hide();
    }
}
