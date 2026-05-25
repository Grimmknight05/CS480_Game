using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitalCamera : MonoBehaviour
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
    // =====================================================================
// ADD these fields and methods to the existing OrbitalCamera class.
// =====================================================================

[Header("External Control")]
[Tooltip("When true, the camera ignores mouse input and stays under external control.")]
private bool externalControl = false;

// Saved state for smooth restoration after cutscenes.
private float savedYaw;
private float savedPitch;
private float savedDistance;
private float savedFOV;

// Reference to the Camera component (add if not already present).
private Camera cam;
    /// <summary>
    /// Call before a cutscene begins. Disables orbital updates and stores current
    /// camera settings so they can be restored later.
    /// </summary>
    public void EnableExternalControl()
    {
        if (externalControl) return;

        // Save current orbit angles, distance, and camera FOV.
        savedYaw = currentYaw;
        savedPitch = currentPitch;
        savedDistance = distance;
        if (cam != null) savedFOV = cam.fieldOfView;

        externalControl = true;
    }

    /// <summary>
    /// Call after a cutscene finishes. Re‑enables orbital updates and restores the
    /// camera’s previous state.
    /// </summary>
    public void DisableExternalControl()
    {
        if (!externalControl) return;

        externalControl = false;

        // Restore orbital parameters.
        currentYaw = savedYaw;
        currentPitch = savedPitch;
        distance = savedDistance;

        // Restore FOV.
        if (cam != null) cam.fieldOfView = savedFOV;

        // Force immediate camera update.
        LateUpdate();
    }
    void Awake()
    {
        // Stealing the BossLookAt survival tactic for WebGL
        if (playerRef == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                playerRef = foundPlayer.transform;
                Debug.Log("OrbitalCamera: Player found via Tag in Awake!");
            }
        }
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = GetComponent<Camera>();
    }
    void Start()
    {
        if (playerRef == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                playerRef = found.transform;
            else
                Debug.LogWarning("OrbitalCamera: Player not found yet. Will be assigned later."); // instead of error
        }

        if (playerRef != null)
        {
            currentYaw = playerRef.eulerAngles.y;
            currentPitch = startingPitch;
        }
    }

    void LateUpdate()
    {
        if (playerRef == null) return;
        if (externalControl) return;
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
        if (Physics.SphereCast(targetPosition, cameraRadius, directionToCamera, out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
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