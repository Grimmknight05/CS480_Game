using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class IcePlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera used to orient WASD movement. Falls back to Camera.main.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Optional visual mesh child. Its X/Z rotation is zeroed every frame to keep the player upright.")]
    [SerializeField] private Transform visualModel;

    [Header("Movement")]
    [SerializeField] private float moveForce = 30f;
    [SerializeField] private float maxSpeed = 6f;

    [Header("Friction Boots Toggle")]
    [Tooltip("TRUE = precise, grippy movement (high drag). FALSE = slippery ice physics (low drag).")]
    public bool isFrictionBootsActive = false;
    [SerializeField] private float icyDrag = 0.15f;
    [SerializeField] private float bootsDrag = 6f;
    [Tooltip("Move force is scaled down on ice so max speed isn't reached instantly.")]
    [SerializeField] private float icyForceScale = 0.45f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Low Gravity")]
    [Tooltip("Multiplier on Physics.gravity for this player only. 1 = normal, 0.5 = moon-ish.")]
    [SerializeField] private float gravityMultiplier = 0.45f;

    private Rigidbody rb;
    private SphereCollider sphere;
    private Vector2 moveInput;
    private bool jumpQueued;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Debug.Log($"RB state: kinematic={rb.isKinematic}, useGravity={rb.useGravity}, mass={rb.mass}, constraints={rb.constraints}, drag={rb.linearDamping}");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) { moveInput = Vector2.zero; return; }

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        moveInput = new Vector2(x, y);

        if (kb.spaceKey.wasPressedThisFrame) {
            jumpQueued = true;
            Debug.Log("Space");
        }

        if (visualModel != null)
        {
            Vector3 e = visualModel.eulerAngles;
            visualModel.eulerAngles = new Vector3(0f, e.y, 0f);
        }
    }

    void FixedUpdate()
    {
        rb.linearDamping = 0f;

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right   = cameraTransform != null ? cameraTransform.right   : Vector3.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 desired = forward * moveInput.y + right * moveInput.x;
        if (desired.sqrMagnitude > 1f) desired.Normalize();

        float forceScale = isFrictionBootsActive ? 1f : icyForceScale;
        rb.AddForce(desired * moveForce * forceScale, ForceMode.Acceleration);

        float horizDrag = isFrictionBootsActive ? bootsDrag : icyDrag;
        float dragFactor = 1f / (1f + horizDrag * Time.fixedDeltaTime);
        Vector3 vel = rb.linearVelocity;
        vel.x *= dragFactor;
        vel.z *= dragFactor;

        Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
        if (horiz.magnitude > maxSpeed)
        {
            horiz = horiz.normalized * maxSpeed;
            vel.x = horiz.x;
            vel.z = horiz.z;
        }
        rb.linearVelocity = vel;

        if (jumpQueued)
        {
            jumpQueued = false;
            if (isGrounded)
            {
                Debug.Log($"JUMP fired. velBefore={rb.linearVelocity.y}, mass={rb.mass}");
                Vector3 v = rb.linearVelocity;
                v.y = 0f;
                rb.linearVelocity = v;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                Debug.Log($"JUMP after. velAfter={rb.linearVelocity.y}");
            }
            else
            {
                Debug.Log("JUMP skipped, not grounded at FixedUpdate");
            }
        }

        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        isGrounded = false;
    }

    void OnCollisionStay(Collision c)
    {
        for (int i = 0; i < c.contactCount; i++)
        {
            if (c.GetContact(i).normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log($"Collision ENTER with '{c.gameObject.name}' (layer {LayerMask.LayerToName(c.gameObject.layer)}), contacts={c.contactCount}");
    }

    public void SetFrictionBoots(bool active) => isFrictionBootsActive = active;

    void OnDrawGizmosSelected()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return;
        float scale = Mathf.Max(transform.lossyScale.x,
                      Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        float radius = sc.radius * scale;
        Vector3 origin = transform.position + sc.center;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * (radius + groundCheckDistance));
    }
}
