using UnityEngine;

public class TurnableStoneTest : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private ActivatorID stoneID;
    
    [Header("Channel")]
    [SerializeField] private ActivatorStateChannelTest stateChannel;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float rotationTolerance = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private float currentRotation;
    private float targetRotation;
    private bool isRotating = false;
    
    private void Start()
    {
        currentRotation = transform.eulerAngles.y;
        targetRotation = currentRotation;
        
        // Broadcast initial state
        if (stateChannel != null && stoneID != null)
        {
            stateChannel.Raise(stoneID, currentRotation);
            if (debugMode) Debug.Log($"[Stone] {stoneID.name} start rotation={currentRotation}");
        }
    }
    
    public void Interact()
    {
        if (isRotating) return;
        
        targetRotation = Mathf.Repeat(targetRotation + 90f, 360f);
        isRotating = true;
        if (debugMode) Debug.Log($"[Stone] {stoneID.name} target set to {targetRotation}");
    }
    
    private void Update()
    {
        if (isRotating) RotateToTarget();
    }
    
    private void RotateToTarget()
    {
        float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));
        
        if (diff <= rotationTolerance)
        {
            currentRotation = targetRotation;
            transform.eulerAngles = new Vector3(0, currentRotation, 0);
            isRotating = false;
            
            // Broadcast final rotation
            if (stateChannel != null)
            {
                stateChannel.Raise(stoneID, currentRotation);
                if (debugMode) Debug.Log($"[Stone] {stoneID.name} finished at {currentRotation}");
            }
        }
        else
        {
            float delta = Mathf.DeltaAngle(currentRotation, targetRotation);
            float step = Mathf.Sign(delta) * rotationSpeed * Time.deltaTime;
            if (Mathf.Abs(step) > Mathf.Abs(delta)) step = delta;
            currentRotation = Mathf.Repeat(currentRotation + step, 360f);
            transform.eulerAngles = new Vector3(0, currentRotation, 0);
            
            // Optional: broadcast intermediate states if needed for UI
            // stateChannel.Raise(stoneID, currentRotation);
        }
    }
}