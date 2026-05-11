using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HUDDialogueVisibility : MonoBehaviour
{
    [SerializeField] private CanvasGroup target;
    [SerializeField] private DialogueEventChannelSO startChannel;
    [SerializeField] private DialogueEndedChannelSO endedChannel;

    void Awake()
    {
        if (target == null) target = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (startChannel != null) startChannel.OnRaised += HandleStart;
        if (endedChannel != null) endedChannel.OnRaised += HandleEnded;
    }

    void OnDisable()
    {
        if (startChannel != null) startChannel.OnRaised -= HandleStart;
        if (endedChannel != null) endedChannel.OnRaised -= HandleEnded;
    }

    private void HandleStart(DialogueSO _)
    {
        if (target == null) return;
        target.alpha = 0f;
        target.blocksRaycasts = false;
    }

    private void HandleEnded()
    {
        if (target == null) return;
        target.alpha = 1f;
        target.blocksRaycasts = true;
    }
}
