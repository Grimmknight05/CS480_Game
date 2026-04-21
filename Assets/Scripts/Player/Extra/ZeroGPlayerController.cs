using UnityEngine;
using UnityEngine.InputSystem;

public class ZeroGPlayerController : MonoBehaviour
{
    /* Physics */
    private Rigidbody rb;

    /* Movement Input */
    private float moveX;
    private float moveY;
    private float moveZ; // up/down

    private Vector3 cachedMoveDirection;

    /* Movement Settings */
    [SerializeField] private float playerSpeed = 8f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 16f;

    /* Look */
    [SerializeField] private Transform cameraPivot;

    /* Input */
    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 🔑 Zero-G setup
        rb.useGravity = false;
        rb.linearDamping = 0f; // no automatic slowing
    }

    /* ================= INPUT ================= */

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveX = input.x;
        moveY = input.y;
    }

    // Bind this to Space / Ctrl or triggers
    void OnUpDown(InputValue value)
    {
        moveZ = value.Get<float>();
    }

    // Optional instant stop (HIGHLY recommended)
    void OnBrake(InputValue value)
    {
        if (value.isPressed)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    /* ================= UPDATE ================= */

    void Update()
    {
        // Camera-relative movement
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        Vector3 up = cameraPivot.up;

        cachedMoveDirection = forward * moveY + right * moveX + up * moveZ;

        if (cachedMoveDirection.sqrMagnitude > 1f)
            cachedMoveDirection.Normalize();
    }

    void FixedUpdate()
    {
        Vector3 movement = cachedMoveDirection;

        // Target velocity in full 3D
        Vector3 targetVelocity = movement * playerSpeed;

        float accelRate = (movement.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);

        Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);

        // Clamp total speed (not just horizontal!)
        newVelocity = Vector3.ClampMagnitude(newVelocity, playerSpeed);

        rb.linearVelocity = newVelocity;
    }
}