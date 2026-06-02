using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialComputerInteractable : MonoBehaviour
{
    [SerializeField] private TutorialScreenUI tutorialUI;
    [SerializeField] private InteractionPromptChannelSO promptChannel;
    [SerializeField] private string promptMessage = "Press E for Tutorial";
    [SerializeField] private bool closeScreenOnExit = true;
    [SerializeField] private bool forceTriggerCollider = true;
    [SerializeField] private bool autoCreateTriggerCollider = true;
    [SerializeField] private float triggerRadius = 2.4f;
    [SerializeField] private Vector3 triggerCenter = new Vector3(0f, 0.6f, 0f);

    [Header("Interactable Icon")]
    [SerializeField] private bool autoCreateHolographicIcon = true;
    [SerializeField] private bool hideIconWhileTutorialOpen = true;
    [SerializeField] private string iconLabel = "Tutorial";
    [SerializeField] private float holographicIconScaleMultiplier = 1.8f;
    [SerializeField] private HolographicInteractableIcon holographicIcon;

    private bool playerInRange;
    private bool promptShown;

    private void Reset()
    {
        ConfigureTriggerCollider();
    }

    private void Awake()
    {
        ConfigureTriggerCollider();

        if (tutorialUI == null)
            tutorialUI = FindFirstObjectByType<TutorialScreenUI>(FindObjectsInactive.Include);

        ResolveHolographicIcon();
        RefreshHolographicIcon();
    }

    private void OnEnable()
    {
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;

        if (tutorialUI != null)
            tutorialUI.VisibilityChanged += HandleTutorialVisibilityChanged;

        RefreshHolographicIcon();
    }

    private void OnDisable()
    {
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;

        if (tutorialUI != null)
            tutorialUI.VisibilityChanged -= HandleTutorialVisibilityChanged;

        HidePrompt();

        if (holographicIcon != null)
            holographicIcon.SetHighlighted(false);
    }

    private void HandleInteractPressed()
    {
        if (tutorialUI == null)
            tutorialUI = FindFirstObjectByType<TutorialScreenUI>(FindObjectsInactive.Include);

        if (tutorialUI != null && tutorialUI.IsOpen)
        {
            RefreshHolographicIcon();
            return;
        }

        if (!playerInRange && !IsPlayerCloseEnough())
            return;

        if (tutorialUI != null)
            tutorialUI.Show();

        RefreshHolographicIcon();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (tutorialUI == null)
            tutorialUI = FindFirstObjectByType<TutorialScreenUI>(FindObjectsInactive.Include);

        if (tutorialUI == null || !tutorialUI.IsOpen)
            ShowPrompt();

        RefreshHolographicIcon();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HidePrompt();

        if (closeScreenOnExit && tutorialUI != null && tutorialUI.IsOpen)
            tutorialUI.Hide();

        RefreshHolographicIcon();
    }

    private void HandleTutorialVisibilityChanged(bool isVisible)
    {
        if (playerInRange)
        {
            if (isVisible)
                HidePrompt();
            else
                ShowPrompt();
        }

        RefreshHolographicIcon();
    }

    private void ConfigureTriggerCollider()
    {
        if (autoCreateTriggerCollider)
        {
            SphereCollider rangeTrigger = GetInteractionRangeTrigger();
            rangeTrigger.radius = triggerRadius;
            rangeTrigger.center = triggerCenter;
            rangeTrigger.isTrigger = true;
            return;
        }

        Collider trigger = GetComponent<Collider>();
        if (trigger != null && forceTriggerCollider)
            trigger.isTrigger = true;
    }

    private SphereCollider GetInteractionRangeTrigger()
    {
        SphereCollider[] sphereColliders = GetComponents<SphereCollider>();
        foreach (SphereCollider sphereCollider in sphereColliders)
        {
            if (sphereCollider != null && sphereCollider.isTrigger)
                return sphereCollider;
        }

        return gameObject.AddComponent<SphereCollider>();
    }

    private bool IsPlayerCloseEnough()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return false;

        Vector3 worldCenter = transform.TransformPoint(triggerCenter);
        float largestScale = Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        float worldRadius = Mathf.Max(0.25f, triggerRadius * largestScale);
        return (player.transform.position - worldCenter).sqrMagnitude <= worldRadius * worldRadius;
    }

    private void ResolveHolographicIcon()
    {
        if (!autoCreateHolographicIcon)
            return;

        if (holographicIcon == null)
            holographicIcon = GetComponentInChildren<HolographicInteractableIcon>(true);

        if (holographicIcon == null)
            holographicIcon = gameObject.AddComponent<HolographicInteractableIcon>();

        holographicIcon.SetLabel(iconLabel);
        holographicIcon.SetScaleMultiplier(holographicIconScaleMultiplier);
    }

    private void RefreshHolographicIcon()
    {
        ResolveHolographicIcon();

        if (holographicIcon == null)
            return;

        bool tutorialIsOpen = tutorialUI != null && tutorialUI.IsOpen;
        bool iconVisible = !tutorialIsOpen || !hideIconWhileTutorialOpen;
        holographicIcon.SetVisible(iconVisible);
        holographicIcon.SetHighlighted(iconVisible && playerInRange);
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
