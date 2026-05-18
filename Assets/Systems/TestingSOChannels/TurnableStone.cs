using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot / Architecture Refactor (April 2026)
// David - Altered for InteractableTriggers and removing camera raycasting (4/25/26)
// Sarah - Modified for adding sounds using Cursor (5/17/26)
public class TurnableStone : MonoBehaviour
{
    [Header("Event Channels")]
    [Tooltip("The walkie-talkie channel this stone uses to broadcast its state.")]
    [SerializeField] private FloatActivatorChannel stateChannel;
    [SerializeField] private ActivatorID activatorID;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Adjusted for degree-per-second rotation
    [SerializeField] private float rotationTolerance = 0.5f;

    [Header("Input Buffer")]
    [Tooltip("How many turns can the player queue up by spamming E?")]
    [SerializeField] private int maxQueuedTurns = 3;
    private int currentQueuedTurns = 0;

    [Header("Restrictions")]
    [SerializeField] private bool Inputlock = false;//Lock all input channel
    [SerializeField] private bool Playerlock = false;//Lock for player Channel
    [SerializeField] private GameObject InputlockVisual;
    [SerializeField] private GameObject PlayerlockVisual;
    [Header("Interaction Events")]
    public UnityEvent onInteract;

    [Header("Audio (optional)")]
    [Tooltip("Looped while the stone is rotating; stops each time it settles at a 90° step. Needs an AudioSource on this object (or assign one below).")]
    [SerializeField] private AudioClip turningLoopClip;
    [SerializeField] [Range(0f, 1f)] private float turnSoundVolume = 1f;
    [SerializeField] private AudioSource audioSource;
    [Header("Stone Reference")]
    [SerializeField] private string stoneID;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    private float initialRotationY; // Store the starting Y rotation
    private float currentRotation;
    private float targetRotation;
    private bool isRotating = false;
    private bool hasRaisedEventForCurrentTarget = true;
    public float TargetRotation => targetRotation;
    public float CurrentRotation => currentRotation;
    public string StoneID => stoneID;

    private void Start()
    {
        if (audioSource == null)
            TryGetComponent(out audioSource);

        initialRotationY = transform.eulerAngles.y;//Gets the current rotation
        currentRotation = 0f;//Intialized both currentRotation and targetRotation to 0
        targetRotation = 0f;

        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} initialized. Starting rotation: {initialRotationY}°, Offset: {currentRotation}°");
        SetLockVisablity();
        // Broadcast initial state on startup so the Validator knows where we are
        if (stateChannel != null && activatorID != null)
            stateChannel.RaiseEvent(activatorID, currentRotation);
    }

    private void OnDisable()
    {
        StopTurningLoop();
    }

    // Call this method from your new InteractableTrigger volume
    public void Interact()
    {
        if (Inputlock || Playerlock) return; // If either lock dont let player rotate
        onInteract?.Invoke();
        setTargetRot();
    }
    public void InteractBypassPlayerLock()
    {
        if (Inputlock) return;
        setTargetRot();
    }
    public void SetPlayerLock(bool state)
    {
        Playerlock = state;
        SetLockVisablity();
    }
    public void SetInputLock(bool state)
    {
        Inputlock = state;
        SetLockVisablity();
    }
    public void ToggleInputLock()
    {
        Inputlock = !Inputlock;
        SetLockVisablity();
    }
    public void TogglePlayerLock()
    {
        Inputlock = !Inputlock;
        SetLockVisablity();
    }
    public void SetLockVisablity()
    {
        PlayerlockVisual.SetActive(Playerlock);
        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} Playerlock: {Playerlock}");
        InputlockVisual.SetActive(Inputlock);
        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} Inputlock: {Inputlock}");
    }
    public void setTargetRot()
    {
        // Instead of blocking interaction entirely, block it if the queue is full
        if (currentQueuedTurns >= maxQueuedTurns) return;

        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} interacted! Turn queued.");


        currentQueuedTurns++;

        // If it's not currently moving, kickstart the rotation process
        if (!isRotating)
        {
            SetNext90DegreeTarget();
        }
    }

    private void SetNext90DegreeTarget()
    {
        // Safely add exactly 90 degrees to our CURRENT physical rotation
        targetRotation = currentRotation + 90f;
        isRotating = true;
        hasRaisedEventForCurrentTarget = false;
        StartTurningLoop();
    }

    private void Update()
    {
        if (isRotating)
        {
            RotateToTarget();
        }
    }

    private void RotateToTarget()
    {
        // Don't use DeltaAngle here, use standard difference so we don't accidentally spin backward
        float difference = Mathf.Abs(targetRotation - currentRotation);

        if (difference <= rotationTolerance)
        {
            // Snap to exact target
            currentRotation = targetRotation;

            // We reached a 90 degree stop. Fire the event for the puzzle/platforms!
            if (!hasRaisedEventForCurrentTarget)
            {
                hasRaisedEventForCurrentTarget = true;


                // Normalize for the broadcast (e.g. 360 becomes 0) so the Puzzle Validator understands it
                float normalizedRotation = Mathf.Repeat(currentRotation, 360f);
                if (stateChannel != null && activatorID != null)
                    stateChannel.RaiseEvent(activatorID, normalizedRotation);


            }

            // We finished one turn. Remove it from the queue.
            currentQueuedTurns--;

            // If the player spammed E and we have more turns in the queue, immediately start the next one
            if (currentQueuedTurns > 0)
            {
                SetNext90DegreeTarget();
            }
            else
            {
                // Queue is empty, finally stop resting.
                StopTurningLoop();
                isRotating = false;
                // Normalize the underlying math variables so they don't climb to infinity
                currentRotation = Mathf.Repeat(currentRotation, 360f);
                targetRotation = currentRotation;
            }

            // Apply final physical transform
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRotationY + currentRotation, transform.eulerAngles.z);
        }
        else
        {
            // Interpolate rotation smoothly forward
            float rotationStep = rotationSpeed * Time.deltaTime;

            // Prevent overshooting
            if (rotationStep > difference)
            {
                rotationStep = difference;
            }

            currentRotation += rotationStep;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRotationY + currentRotation, transform.eulerAngles.z);
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource != null)
            audioSource.PlayOneShot(clip, turnSoundVolume);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position, turnSoundVolume);
    }

    private void StartTurningLoop()
    {
        if (turningLoopClip == null || audioSource == null) return;
        if (audioSource.isPlaying && audioSource.loop && audioSource.clip == turningLoopClip)
            return;

        audioSource.loop = true;
        audioSource.clip = turningLoopClip;
        audioSource.volume = turnSoundVolume;
        audioSource.Play();
    }

    private void StopTurningLoop()
    {
        if (audioSource == null || turningLoopClip == null) return;
        if (!audioSource.loop || audioSource.clip != turningLoopClip) return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }
}
