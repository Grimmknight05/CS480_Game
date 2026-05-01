using Unity.VisualScripting;
using UnityEngine;

// Author: Joshua Henrikson
// Modified by: GitHub Copilot / Architecture Refactor (April 2026)
// David - Altered for InteractableTriggers and removing camera raycasting (4/25/26)
public class TurnableStone : MonoBehaviour
{
    [Header("Event Channels")]
    [Tooltip("The walkie-talkie channel this stone uses to broadcast its state.")]
    [SerializeField] private ActivatorStateChannel stateChannel;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Adjusted for degree-per-second rotation
    [SerializeField] private float rotationTolerance = 0.5f;
    
    [Header("Restrictions")]
    [SerializeField] private bool Inputlock = false;//Lock all input channel
    [SerializeField] private bool Playerlock = false;//Lock for player Channel
    [SerializeField] private GameObject InputlockVisual;
    [SerializeField] private GameObject PlayerlockVisual;

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
        initialRotationY = transform.eulerAngles.y;//Gets the current rotation
        currentRotation = 0f;//Intialized both currentRotation and targetRotation to 0
        targetRotation = 0f;
        
        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} initialized. Starting rotation: {initialRotationY}°, Offset: {currentRotation}°");
        SetLockVisablity();    
        // Broadcast initial state on startup so the Validator knows where we are
        if (stateChannel != null)
            stateChannel.RaiseEvent(stoneID, currentRotation);
    }

    // Call this method from your new InteractableTrigger volume
    public void Interact()
    {
        if (Inputlock || Playerlock) return; // If either lock dont let player rotate
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
        if (isRotating) return; // Prevent spamming while it's already turning
        
        if (debugMode)
            Debug.Log($"[TurnableStone] {stoneID} interacted! Setting new target.");
        
        targetRotation += 90f;
        targetRotation = Mathf.Repeat(targetRotation, 360f);
        
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
        float difference = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));
        
        if (difference <= rotationTolerance)
        {
            // Snap to exact target and stop rotating
            currentRotation = targetRotation;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRotationY + currentRotation, transform.eulerAngles.z);
            isRotating = false;

            // Fire the event ONLY ONCE when rotation finishes
            if (!hasRaisedEventForCurrentTarget)
            {
                hasRaisedEventForCurrentTarget = true;
                if (stateChannel != null)
                    stateChannel.RaiseEvent(stoneID, currentRotation);
            }
        }
        else
        {
            // Interpolate rotation smoothly
            float rotationDelta = Mathf.DeltaAngle(currentRotation, targetRotation);
            float rotationStep = Mathf.Sign(rotationDelta) * rotationSpeed * Time.deltaTime;

            // Prevent overshooting the target
            if (Mathf.Abs(rotationStep) > Mathf.Abs(rotationDelta))
            {
                rotationStep = rotationDelta;
            }

            currentRotation += rotationStep;
            currentRotation = Mathf.Repeat(currentRotation, 360f);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRotationY + currentRotation, transform.eulerAngles.z);
        }
    }
}