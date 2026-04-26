using UnityEngine;
using UnityEngine.InputSystem;

public class FPSCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 180f;
    [SerializeField] private float maxPitch = 80f;

    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    /* ============================================================
     * Pauses camera look while a conversation is active so the
     * mouse can't rotate the camera (and so a stale look delta
     * from an input-focus blip doesn't snap the view).
     * ============================================================ */
    [Header("Dialogue")]
    [SerializeField] private DialogueEventChannelSO dialogueStartedChannel;
    [SerializeField] private DialogueEndedChannelSO dialogueEndedChannel;
    private bool dialogueLock = false;

    void OnEnable()
    {
        if (dialogueStartedChannel != null) dialogueStartedChannel.OnRaised += HandleDialogueStarted;
        if (dialogueEndedChannel != null)   dialogueEndedChannel.OnRaised   += HandleDialogueEnded;
    }

    void OnDisable()
    {
        if (dialogueStartedChannel != null) dialogueStartedChannel.OnRaised -= HandleDialogueStarted;
        if (dialogueEndedChannel != null)   dialogueEndedChannel.OnRaised   -= HandleDialogueEnded;
    }

    private void HandleDialogueStarted(DialogueSO _)
    {
        dialogueLock = true;
        lookInput = Vector2.zero;
    }

    private void HandleDialogueEnded()
    {
        dialogueLock = false;
        lookInput = Vector2.zero;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (dialogueLock)
        {
            // Defensive: even if OnLook fires while paused, ignore it.
            lookInput = Vector2.zero;
            return;
        }
        HandleLook();
    }

    void HandleLook()
    {
        Vector2 delta = lookInput * sensitivity * Time.deltaTime;

        yaw += delta.x;
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        // Reset so a stale value can't keep rotating the camera if the
        // input system briefly stops sending OnLook events (e.g. when
        // input focus shifts as the dialogue panel activates).
        lookInput = Vector2.zero;
    }

    void LateUpdate()
    {
        // IMPORTANT: LateUpdate = eliminates jitter completely
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void OnLook(InputValue value)
    {
        if (dialogueLock) return;
        lookInput = value.Get<Vector2>();
    }
}
