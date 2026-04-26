// =====================================================================
// Typed event channel used to start a conversation. NPC triggers raise
// a DialogueSO on this channel, and the scene's DialogueRunner listens
// for it. Built on top of the project's existing EventChannelSO<T>
// pattern from Assets/Scripts/Events/.
// =====================================================================

using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Events/Dialogue Event Channel",
                 fileName = "DialogueEventChannel")]
public class DialogueEventChannelSO : EventChannelSO<DialogueSO>
{
}
