// =====================================================================
// Author: Katie Trinh
// Concrete Command that ends the active conversation immediately.
// Useful for a "close" button, a scene transition, or any system
// that needs to force-end dialogue without knowing the internals.
// =====================================================================

public class EndDialogueCommand : IDialogueCommand
{
    private DialogueRunner runner;

    public EndDialogueCommand(DialogueRunner runner)
    {
        this.runner = runner;
    }

    public void Execute()
    {
        runner.EndConversation();
    }
}
