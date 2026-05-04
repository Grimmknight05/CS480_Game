using UnityEngine;
using UnityEngine.InputSystem;

public class UnifiedOrbitalCamera : MonoBehaviour
{
    [Header("Camera Collision")]
    public LayerMask collisionLayers; // Set this in the Inspector to "Default" or your Environment layers
    public float cameraRadius = 0.3f; // How thick the camera is

    [Header("Targeting")]
    public Transform playerRef;
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Aim at the shoulders/head, not the feet

    [Header("Camera Distance")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;

    [Header("Orbit Speeds")]
    public float yawSpeed = 0.2f;
    public float pitchSpeed = 0.2f;

    [Header("Pitch Limits")]
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float currentYaw;
    private float currentPitch;

    [Header("Starting Perspective")]
    public float startingPitch = 20f; // 20 degrees looking down

    void Start()
    {
        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Grab the initial Y rotation from the player so we start facing their back
        currentYaw = playerRef.eulerAngles.y; 
        
        // Force the classic "slightly above" tilt
        currentPitch = startingPitch; 
    }

    void LateUpdate()
    {
        if (playerRef == null) return;

        // 1. Get Mouse Input (Using your Mouse.current method)
        Mouse m = Mouse.current;
        if (m != null)
        {
            float mouseX = m.delta.ReadValue().x;
            float mouseY = m.delta.ReadValue().y;

            currentYaw += mouseX * yawSpeed;
            currentPitch -= mouseY * pitchSpeed; // Subtract to avoid inverted Y axis
            
            // Clamp the pitch so the camera doesn't flip over the player's head
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        }

        // 2. Calculate the new Rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // 3. Calculate the new Position & Apply Collision
        Vector3 targetPosition = playerRef.position + targetOffset;
        
        // Where the camera WANTS to be
        Vector3 desiredPosition = targetPosition + rotation * new Vector3(0, 0, -distance);
        
        // The direction from the player's head to the desired camera spot
        Vector3 directionToCamera = (desiredPosition - targetPosition).normalized;

        // Shoot a thick laser (SphereCast) from the player to the camera
        if (Physics.SphereCast(targetPosition, cameraRadius, directionToCamera, out RaycastHit hit, distance, collisionLayers))
        {
            // If the laser hits a wall, snap the camera to just in front of that wall
            transform.position = hit.point + hit.normal * 0.1f; // The 0.1f offset prevents near-clipping the wall texture
        }
        else
        {
            // If the laser hits nothing, put the camera at the normal distance
            transform.position = desiredPosition;
        }

        // 4. Apply the Rotation
        transform.rotation = rotation;
    }
}