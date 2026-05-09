// =====================================================================
// Concrete Command that starts a dialogue conversation.
// Wraps the act of raising the start channel so any system
// (trigger collider, cutscene, quest) can start dialogue
// without knowing about channels directly.
// =====================================================================

public class StartDialogueCommand : IDialogueCommand
{
    private DialogueEventChannelSO startChannel;
    private DialogueSO dialogue;

    public StartDialogueCommand(DialogueEventChannelSO startChannel, DialogueSO dialogue)
    {
        this.startChannel = startChannel;
        this.dialogue = dialogue;
    }

    public void Execute()
    {
        if (startChannel != null && dialogue != null)
        {
            startChannel.Raise(dialogue);
        }
    }
}
