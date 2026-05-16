using UnityEngine;

public readonly struct InteractionPromptData
{
    public readonly Component Source;
    public readonly bool Visible;
    public readonly string Message;

    public InteractionPromptData(Component source, bool visible, string message)
    {
        Source = source;
        Visible = visible;
        Message = message;
    }
}

[CreateAssetMenu(menuName = "Events/Interaction Prompt Channel",
                 fileName = "InteractionPromptChannel")]
public class InteractionPromptChannelSO : EventChannelSO<InteractionPromptData>
{
}
