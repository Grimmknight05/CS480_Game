// =====================================================================
// Concrete Command that skips the typewriter animation and shows
// all of the current line's text immediately.
// =====================================================================

public class SkipRevealCommand : IDialogueCommand
{
    private DialogueRunner runner;

    public SkipRevealCommand(DialogueRunner runner)
    {
        this.runner = runner;
    }

    public void Execute()
    {
        runner.CompleteCurrentReveal();
    }
}
