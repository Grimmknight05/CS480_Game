using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class TurnableStone : MonoBehaviour
{

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationTolerance = 2f;
    [SerializeField] private float targetRotation;//What rotation stone needs to be facing
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float facingThreshold = 0.5f;

    [SerializeField] private TurnableStoneManager manager;
    [Header("Stone Reference")]
    [SerializeField] private string stoneID;
    
    
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    //Private variables
    private float currentRotation;
    private bool isRotating = false;
    private Transform playerCamera;
    private bool isAlignedLastFrame = false;

    //public events
    public event Action<TurnableStone> OnReachedTarget;

    // Expose variables
    public float TargetRotation => targetRotation;
    public float CurrentRotation => currentRotation;
    public float RotationTolerance => rotationTolerance;
    public bool IsRotating => isRotating;
    public string StoneID => stoneID;
    
    private void Start()
    {
        currentRotation = transform.eulerAngles.y;
        Debug.Log($"Stone loaded: {StoneID}");
        if (debugMode)
            Debug.Log($"[TurnableStone] {gameObject.name} initialized. Current rotation: {currentRotation}");
        
        if (manager == null)
        {
            Debug.LogError($"[TurnableStone] No manager assigned on {gameObject.name}!");
            return;
        }
        manager.RegisterStone(this);
        
        // Auto-assign camera (since there's only one main camera)
        if (playerCamera == null)
        {
            playerCamera = FindAnyObjectByType<Camera>()?.transform;//Assign player camera
            
            if (playerCamera == null)
            {
                Debug.LogError("[TurnableStone] No camera found in scene!");
                return;
            }
            
            if (debugMode)
                Debug.Log($"[TurnableStone] Camera auto-assigned: {playerCamera.name}");
        }
        else
        {
            if (debugMode)
                Debug.Log($"[TurnableStone] Camera already assigned: {playerCamera.name}");
        }
    }
    public void OnInteractInput()
    {
        Debug.Log($"[TurnableStone] {gameObject.name} received OnInteractInput call!");
        
        if (playerCamera == null)
        {
            Debug.LogWarning($"[TurnableStone] Player camera is NULL!");
            return;
        }
        
        bool inRange = IsPlayerInRange();
        bool facing = IsPlayerFacingStone();
        
        Debug.Log($"[TurnableStone] InRange: {inRange}, Facing: {facing}");
        
        if (inRange && facing)
        {
            OnStoneInteracted();
        }
    }

    
    private void OnDestroy()
    {
        if (manager != null)
            manager.UnregisterStone(this);
    }
    
    public void OnInteract(InputValue inputValue)
    {
        if (debugMode)
            Debug.Log($"[TurnableStone] OnInteract called! isPressed: {inputValue.isPressed}");
        
        if (inputValue.isPressed)
        {
            if (playerCamera == null)
            {
                if (debugMode)
                    Debug.LogWarning($"[TurnableStone] Player camera is NULL! Cannot check interaction.");
                return;
            }
            
            bool inRange = IsPlayerInRange();
            bool facing = IsPlayerFacingStone();
            
            if (debugMode)
            {
                Debug.Log($"[TurnableStone] Interaction check: InRange={inRange}, Facing={facing}");
            }
            
            if (inRange && facing)
            {
                OnStoneInteracted();
            }
            else
            {
                if (debugMode)
                {
                    if (!inRange)
                        Debug.Log($"[TurnableStone] Too far away!");
                    if (!facing)
                        Debug.Log($"[TurnableStone] Not facing the stone!");
                }
            }
        }
    }
    
    private bool IsPlayerInRange()
    {
        float distance = Vector3.Distance(playerCamera.position, transform.position);
        
        if (debugMode)
            Debug.Log($"[TurnableStone] Distance to player: {distance:F2} (Threshold: {interactionDistance})");
        
        return distance <= interactionDistance;
    }
    
    private bool IsPlayerFacingStone()
    {
        Vector3 directionToStone = (transform.position - playerCamera.position).normalized;
        Vector3 playerForward = playerCamera.forward;
        float dotProduct = Vector3.Dot(playerForward, directionToStone);
        
        if (debugMode)
            Debug.Log($"[TurnableStone] Dot product: {dotProduct:F2} (Threshold: {facingThreshold})");
        
        return dotProduct >= facingThreshold;
    }
    
    private void OnStoneInteracted()
    {
        if (debugMode)
            Debug.Log($"[TurnableStone] ✓ Stone successfully interacted! Setting target rotation: {targetRotation}");
        
        // Actually rotate the stone - for example, rotate 90 degrees on each interaction
        targetRotation += 90f;
        targetRotation = Mathf.Repeat(targetRotation, 360f);
        
        if (debugMode)
            Debug.Log($"[TurnableStone] New target rotation: {targetRotation}");
    }
    
    public bool IsAtTargetRotation()
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));
        return difference <= rotationTolerance;
    }
    
    public void RotateToTarget()
    {
        if (IsAtTargetRotation())
        {
            isRotating = false;
        
        }
        else
        {
            isRotating = true;
            float rotationDelta = Mathf.DeltaAngle(currentRotation, targetRotation);
            float rotationDirection = Mathf.Sign(rotationDelta);
            
            currentRotation += rotationDirection * rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.Repeat(currentRotation, 360f);
            
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotation, transform.eulerAngles.z);
        }
    }
    
    public void SetTargetRotation(float rotation)
    {
        targetRotation = Mathf.Repeat(rotation, 360f);
        
        if (debugMode)
            Debug.Log($"[TurnableStone] Target rotation set to: {targetRotation}");
    }
    private void CheckAlignmentEvent()
    {
        bool isAligned =
            Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation)) <= rotationTolerance;

        // fire ONLY when transitioning into aligned state
        if (isAligned && !isAlignedLastFrame)
        {
            OnReachedTarget?.Invoke(this);

            if (debugMode)
                Debug.Log($"[{StoneID}] Reached target rotation!");
        }

        isAlignedLastFrame = isAligned;
    }
    
    private void Update()
    {
        RotateToTarget();
        CheckAlignmentEvent();
    }
}
