// =====================================================================
// Interface for the Command pattern applied to the dialogue system.
// Every dialogue command implements Execute(), which decouples
// *what* happens from *when* or *who* triggers it.
// =====================================================================

public interface IDialogueCommand
{
    void Execute();
}
