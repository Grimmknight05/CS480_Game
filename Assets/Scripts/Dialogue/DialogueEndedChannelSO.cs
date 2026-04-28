// =====================================================================
// Void event channel raised when the active conversation finishes.
// The DialogueRunner raises it; the player and any other systems that
// pause/resume during dialogue (cameras, music, AI) can subscribe.
// Built on top of the project's existing VoidEventChannelSO pattern.
// =====================================================================

using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Events/Dialogue Ended Channel",
                 fileName = "DialogueEndedChannel")]
public class DialogueEndedChannelSO : VoidEventChannelSO
{
}
