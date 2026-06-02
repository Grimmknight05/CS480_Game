using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FuelTerminalInteractable : MonoBehaviour
{
    [SerializeField] private FuelTerminalUI terminalUI;
    [SerializeField] private InteractionPromptChannelSO promptChannel;
    [SerializeField] private string promptMessage = "Press E to Check Fuel";
    [SerializeField] private bool closeScreenOnExit = true;
    [SerializeField] private bool forceTriggerCollider = true;

    private bool playerInRange;
    private bool promptShown;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        if (forceTriggerCollider && trigger != null)
            trigger.isTrigger = true;

        if (terminalUI == null)
            terminalUI = FindFirstObjectByType<FuelTerminalUI>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;

        if (terminalUI != null)
            terminalUI.VisibilityChanged += HandleTerminalVisibilityChanged;
    }

    private void OnDisable()
    {
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;

        if (terminalUI != null)
            terminalUI.VisibilityChanged -= HandleTerminalVisibilityChanged;

        HidePrompt();
    }

    private void HandleInteractPressed()
    {
        if (!playerInRange || terminalUI == null)
            return;

        terminalUI.Toggle();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (terminalUI == null)
            terminalUI = FindFirstObjectByType<FuelTerminalUI>(FindObjectsInactive.Include);

        if (terminalUI == null || !terminalUI.IsOpen)
            ShowPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HidePrompt();

        if (closeScreenOnExit && terminalUI != null && terminalUI.IsOpen)
            terminalUI.Hide();
    }

    private void HandleTerminalVisibilityChanged(bool isVisible)
    {
        if (!playerInRange)
            return;

        if (isVisible)
            HidePrompt();
        else
            ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (promptChannel == null || promptShown)
            return;

        promptChannel.Raise(new InteractionPromptData(this, true, promptMessage));
        promptShown = true;
    }

    private void HidePrompt()
    {
        if (promptChannel == null || !promptShown)
            return;

        promptChannel.Raise(new InteractionPromptData(this, false, string.Empty));
        promptShown = false;
    }
}
