// =====================================================================
// Concrete Command that advances the active conversation.
// =====================================================================

public class AdvanceDialogueCommand : IDialogueCommand
{
    private DialogueRunner runner;

    public AdvanceDialogueCommand(DialogueRunner runner)
    {
        this.runner = runner;
    }

    public void Execute()
    {
        runner.TryAdvance();
    }
}
