using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot / Architecture Refactor (April 2026)
// David - Altered for InteractableTriggers and removing camera raycasting (4/25/26)
public class TurnableStone : ResettableBehaviour
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
    [Header("Stone Reference")]
    [SerializeField] private string stoneID;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioClip interactionSound;      // Played when Interact() is called
    [SerializeField] private AudioClip slidingLoop;           // Played while rotating (loop)
    [SerializeField] private AudioClip incrementSound;        // Played after each completed 90° tur
    [SerializeField] private AudioClip lockDispelled;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    private float initialRotationY; // Store the starting Y rotation
    private float currentRotation;
    private float targetRotation;
    private bool isRotating = false;
    private bool hasRaisedEventForCurrentTarget = true;
    public float TargetRotation => targetRotation;
    public float CurrentRotation => currentRotation;
    public float NormalizedRotation => Mathf.Repeat(currentRotation, 360f);
    public bool IsRotating => isRotating;
    public ActivatorID ActivatorID => activatorID;
    public string StoneID => stoneID;
    private bool initialPlayerLock;
    private bool initialInputLock;

    
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (loopSource == null)
            loopSource = gameObject.AddComponent<AudioSource>();
            
        initialRotationY = transform.eulerAngles.y;//Gets the current rotation
        currentRotation = 0f;//Intialized both currentRotation and targetRotation to 0
        targetRotation = 0f;
        initialPlayerLock = Playerlock;
        initialInputLock = Inputlock;
        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} initialized. Starting rotation: {initialRotationY}°, Offset: {currentRotation}°");
        SetLockVisablity();    
        // Broadcast initial state on startup so the Validator knows where we are
        if (stateChannel != null && activatorID != null)
            stateChannel.RaiseEvent(activatorID, currentRotation);
    }

    // Call this method from your new InteractableTrigger volume
    public void Interact()
    {
        if (Inputlock || Playerlock) return; // If either lock dont let player rotate
        onInteract?.Invoke();
        safelyPlayOneShot(interactionSound);
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
        if (state == false){safelyPlayOneShot(lockDispelled);}
        SetLockVisablity();
    }
    public void SetInputLock(bool state)
    {
        Inputlock = state;
        if (state == false){safelyPlayOneShot(lockDispelled);}
        SetLockVisablity();
    }
    public void ToggleInputLock()
    {
        Inputlock = !Inputlock;
        if (Inputlock == false){safelyPlayOneShot(lockDispelled);}
        SetLockVisablity();
    }
    public void TogglePlayerLock()
    {
        Playerlock = !Playerlock;
        if (Playerlock == false){safelyPlayOneShot(lockDispelled);}
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
    public void safelyPlayOneShot(AudioClip audioClip)
    {
        if (audioSource != null && audioClip != null)
            audioSource.PlayOneShot(audioClip);
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
        // Start sliding loop if not already playing
        if (loopSource != null && slidingLoop != null && !isRotating)
        {
            loopSource.clip = slidingLoop;
            loopSource.loop = true;
            loopSource.Play();
            if (debugMode)
                Debug.Log($"[TurnableStone] {stoneID} started sliding loop.");
        }
        // Safely add exactly 90 degrees to our CURRENT physical rotation
        targetRotation = currentRotation + 90f; 
        isRotating = true;
        hasRaisedEventForCurrentTarget = false;
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
                {
                    stateChannel.RaiseEvent(activatorID, normalizedRotation);
                    if (debugMode) Debug.Log($"[TurnableStone] {stoneID} completed turn. Raised {activatorID.name} = {normalizedRotation} on {stateChannel.name}.", this);
                }
                else if (debugMode)
                {
                    Debug.LogWarning($"[TurnableStone] {stoneID} completed turn but is missing a state channel or ActivatorID.", this);
                }
                // Play the 90-degree increment sound
                safelyPlayOneShot(incrementSound);
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
                isRotating = false;
                // Normalize the underlying math variables so they don't climb to infinity
                currentRotation = Mathf.Repeat(currentRotation, 360f);
                targetRotation = currentRotation; 

                if (loopSource != null && loopSource.isPlaying && loopSource.clip == slidingLoop)
                {
                    loopSource.Stop();
                    if (debugMode)
                        Debug.Log($"[TurnableStone] {stoneID} stopped sliding loop.");
                }
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
    // ===== ResettableBehaviour implementation =====
    protected override void ResetInternal()
    {
        // Stop any ongoing rotation
        isRotating = false;
        currentQueuedTurns = 0;
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
        // Reset rotation offset to 0
        currentRotation = 0f;
        targetRotation = 0f;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRotationY, transform.eulerAngles.z);
        hasRaisedEventForCurrentTarget = true;
        // Reset lock states to initial values
        Playerlock = initialPlayerLock;
        Inputlock = initialInputLock;
        SetLockVisablity();
        // Broadcast the reset state (0°)
        if (stateChannel != null && activatorID != null)
            stateChannel.RaiseEvent(activatorID, 0f);
        if (debugMode) Debug.Log($"[TurnableStone] {stoneID} reset to initial rotation.");
    }
}
