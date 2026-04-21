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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
    }

    void HandleLook()
    {
        Vector2 delta = lookInput * sensitivity * Time.deltaTime;

        yaw += delta.x;
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
    }

    void LateUpdate()
    {
        // IMPORTANT: LateUpdate = eliminates jitter completely
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
}