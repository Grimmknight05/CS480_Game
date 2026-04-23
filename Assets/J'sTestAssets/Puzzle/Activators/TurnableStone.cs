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

    [Header("Stone Reference")]
    [SerializeField] private string stoneID;
    
    
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    //Private variables
    private float currentRotation;
    private bool isRotating = false;
    private Transform playerCamera;
    //public events

    // Expose variables
    public float TargetRotation => targetRotation;
    public float CurrentRotation => currentRotation;
    public float RotationTolerance => rotationTolerance;
    public bool IsRotating => isRotating;
    public string StoneID => stoneID;
    
    private void Start()
    {
        currentRotation = transform.eulerAngles.y;
        //Debug.Log($"Stone loaded: {StoneID}");
        if (debugMode)
            Debug.Log($"[TurnableStone] {gameObject.name} initialized. Current rotation: {currentRotation}");
        // Auto-assign camera (since there's only one main camera)
        if (playerCamera == null)
        {
            playerCamera = FindAnyObjectByType<Camera>()?.transform;//Assign player camera
            
            if (playerCamera == null)
            {
                Debug.LogError("[TurnableStone] No camera found in scene!");
                return;
            }
        }

    }
    public void OnInteractInput()
    {
        
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
    
    public void OnInteract(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (playerCamera == null)
            {
                return;
            }
            
            bool inRange = IsPlayerInRange();
            bool facing = IsPlayerFacingStone();
            
            if (inRange && facing)
            {
                OnStoneInteracted();
            }
        }
    }
        private void OnStoneInteracted()
    {
        if (debugMode)
            Debug.Log($"[TurnableStone] ✓ Stone successfully interacted! Setting target rotation: {targetRotation}");
        
        // Actually rotate the stone - for example, rotate 90 degrees on each interaction
        targetRotation += 90f;
        targetRotation = Mathf.Repeat(targetRotation, 360f);
        
    }
    
    //Check if player can interact
    private bool IsPlayerInRange()
    {
        float sqrDistance = (playerCamera.position - transform.position).sqrMagnitude;//Non sqrt distance(optimal)
        return sqrDistance <= interactionDistance * interactionDistance;
    }
    
    private bool IsPlayerFacingStone()//finds dot product of player to ensure proper look direction
    {
        Vector3 directionToStone = (transform.position - playerCamera.position).normalized;
        Vector3 playerForward = playerCamera.forward;
        float dotProduct = Vector3.Dot(playerForward, directionToStone);
        
        return dotProduct >= facingThreshold;
    }
    
    //Rotate Functions
    public bool IsAtTargetRotation()
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));//rotation difference between currentRotation and targetRotation
        return difference <= rotationTolerance;
    }
    
    public void RotateToTarget()
    {
        if (IsAtTargetRotation())
        {
            isRotating = false;//Not part of main function only for public viewing
        
        }
        else
        {
            isRotating = true;//Same here
            float rotationDelta = Mathf.DeltaAngle(currentRotation, targetRotation);//Finds the shortest path/direction to travel between the two angles
            float rotationStep = rotationSpeed * Time.deltaTime;//Amount of rotation per step

            currentRotation += Mathf.Clamp(rotationDelta, -rotationStep, rotationStep);//Clamp rotationDelta within rotationStep and -rotationStep
            currentRotation = Mathf.Repeat(currentRotation, 360f);//Wraps rotation within 360 degrees
            
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotation, transform.eulerAngles.z);//Applies the rotation to the object with calculated currentRotation
        }
    }
    
    public void SetTargetRotation(float rotation)
    {
        targetRotation = Mathf.Repeat(rotation, 360f);//sets new target rotation taking into acount wrapping
    }
    private void CheckAlignmentEvent()//Called in update
    {
        bool isAligned =
            Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation)) <= rotationTolerance;

        GameEvents.RaiseStoneRotationChanged(stoneID, currentRotation);// RaiseStoneRotationChanged Event on GameEvents passing in an id and current rotation so it can be checked for activation
    }
    
    private void Update()
    {
        RotateToTarget();
        CheckAlignmentEvent();
    }
}
