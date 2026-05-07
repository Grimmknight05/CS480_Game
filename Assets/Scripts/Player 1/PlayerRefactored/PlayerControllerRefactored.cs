using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControllerRefactored : MonoBehaviour
{
    [Header("Look & Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Health")]
    private PlayerHealth playerHealth;

    [Header("Physics")]
    public Rigidbody rb;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI winUI;
    [SerializeField] private TextMeshProUGUI deathUI;
    [SerializeField] private GameObject HUD;

    [Header("Score")]
    private int playerPoints;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] jumpSFX;
    [SerializeField] private AudioClip[] airJumpSFX;

    //[Header("Events")]
    public delegate void ScoreChangedDelegate(int newScore);
    public event ScoreChangedDelegate OnScoreChanged;
    public delegate void DeathDelegate();
    public event DeathDelegate OnPlayerDeath;

    [Header("Movement")]
    [SerializeField] private MovementMode initialMode = MovementMode.AccelerationBased;
    private MovementState currentState;
    private float moveX, moveY, moveZ;
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float zgAcceleration = 1f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float zgDeceleration = 0f;
    [SerializeField] private float maxWalkableSlopeAngle = 45f;

    [Header("Jump")]
    [SerializeField] private int maxInAirjumps = 1;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airJumpForce = 8f;
    [SerializeField] private bool canJump = true;
    private CapsuleCollider capsule;
    private bool onGround = false;
    private Vector3 groundNormal = Vector3.up;
    private LayerMask jumpable;
    private bool wasGrounded;

    [Header("Look & Camera")]
    [SerializeField] private Transform cameraPivot;
    private Vector3 cachedMoveDirection;

    private PlayerInput playerInput;
    public Animator animator;

    // Public properties for other states to read
    public bool OnGround => onGround;
    public Vector3 GroundNormal => groundNormal;
    public float PlayerSpeed => playerSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float ZGAcceleration => zgAcceleration;
    public float ZGDeceleration => zgDeceleration;
    public float MaxWalkableSlopeAngle => maxWalkableSlopeAngle;
    public bool CanJump => canJump;
    public JumpAbility jumpAbility;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        playerHealth = GetComponent<PlayerHealth>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        HUD.SetActive(true);
        jumpable = LayerMask.GetMask("Jumpable");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerHealth != null)
            playerHealth.OnPlayerDeath.AddListener(OnHealthDeath);

        jumpAbility = new JumpAbility(maxInAirjumps, jumpForce, airJumpForce, jumpSFX, airJumpSFX, audioSource);

        // Set initial state based on inspector choice
        if (initialMode == MovementMode.ZeroGrav)
            SetMovementState(new ZeroGMovementState());
        else
            SetMovementState(new GroundedMovementState());
    }

    public void SetMovementState(MovementState newState)
    {
        if (currentState == newState) return;
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    // Public method to switch movement mode (mirrors original SetMovementMode)
    public void SetMovementMode(MovementMode mode)
    {
        if (mode == MovementMode.ZeroGrav)
            SetMovementState(new ZeroGMovementState());
        else
            SetMovementState(new GroundedMovementState());
    }

    // Input callbacks
    void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();
        moveX = v.x;
        moveY = v.y;
    }

    void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (jumpAbility.CanExecute(this))
            jumpAbility.Execute(this);
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log($"[PlayerController] OnInteract called, isPressed: {value.isPressed}");
    }

    // Ground detection (exact copy from original)
    void checkGround()
    {
        if (capsule == null) return;

        float radius = capsule.radius * 0.95f;
        float castDistance = (capsule.height * 0.5f) - capsule.radius + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        bool newGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDistance, jumpable);
        onGround = newGrounded;
        groundNormal = newGrounded ? hit.normal : Vector3.up;

        // Reset air jumps only when landing (original resets on CollisionEnter, but we keep both for exactness)
        if (newGrounded && !wasGrounded)
            jumpAbility.ResetOnGround();

        wasGrounded = newGrounded;
        Debug.DrawRay(origin, Vector3.down * castDistance, newGrounded ? Color.green : Color.red);
    }

    // Wall sticking prevention – identical to original
    public void PreventWallSticking(ref Vector3 velocity)
    {
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude < 0.0001f) return;

        Vector3 origin = transform.TransformPoint(capsule.center);
        float radius = capsule.radius * 0.95f;
        float distance = 0.1f;
        int mask = ~LayerMask.GetMask("Player");

        if (Physics.SphereCast(origin, radius, horizontal.normalized, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y > 0.5f) return;

            if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                float into = Vector3.Dot(horizontal, -hit.normal);
                if (into > 0f) horizontal += hit.normal * into;
                return;
            }

            if (Vector3.Dot(horizontal, hit.normal) < 0f)
                horizontal = Vector3.ProjectOnPlane(horizontal, hit.normal);
        }

        velocity.x = horizontal.x;
        velocity.z = horizontal.z;
    }

    // Ground movement logic – exact copy of original's AccelerationBased branch
    public void HandleGroundMovement()
    {
        Vector3 movement = cachedMoveDirection;
        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Vector3 targetVelocity = movement * playerSpeed;

        float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        bool ascending = rb.linearVelocity.y > 0.1f;
        bool onWalkableSlope = onGround && !ascending && slopeAngle > 0.1f && slopeAngle <= maxWalkableSlopeAngle;

        if (onWalkableSlope)
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
        else
            targetVelocity.y = rb.linearVelocity.y;

        float accelRate = (movement.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);
        Vector3 velocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);

        Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
        horizontal = Vector3.ClampMagnitude(horizontal, playerSpeed);
        velocity = new Vector3(horizontal.x, velocity.y, horizontal.z);

        // Anti-slide on walkable slopes with no input
        if (onWalkableSlope && cachedMoveDirection.sqrMagnitude < 0.01f)
            velocity = Vector3.zero;

        PreventWallSticking(ref velocity);
        rb.linearVelocity = velocity;
    }

    // Zero‑G movement – exact copy with exponential smoothing using zgAcceleration/zgDeceleration
    public void HandleZeroGMovement()
    {
        Vector3 movement = cachedMoveDirection;
        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Vector3 targetVelocity = movement * playerSpeed;
        float accelRate = (movement.sqrMagnitude > 0.01f) ? zgAcceleration : zgDeceleration;
        float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);
        Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);
        newVelocity = Vector3.ClampMagnitude(newVelocity, playerSpeed);
        rb.linearVelocity = newVelocity;
    }

    // Rotation – only in non‑ZeroGrav and when input exists
    public void HandleRotation()
    {
        if (currentState is ZeroGMovementState) return;
        Vector3 dir = cachedMoveDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    // Input direction builders (mirror original)
    public void UpdateGroundMovementInput()
    {
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        cachedMoveDirection = forward * moveY + right * moveX;
    }

    public void UpdateZeroGInput()
    {
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        Vector3 up = cameraPivot.up;
        cachedMoveDirection = forward * moveY + right * moveX + up * moveZ;
        if (cachedMoveDirection.sqrMagnitude > 1f)
            cachedMoveDirection.Normalize();
    }

    // Normalized speed for animator (exact original)
    float NormalizeVelocity()
    {
        float horizontalSpeedSqr = rb.linearVelocity.x * rb.linearVelocity.x + rb.linearVelocity.z * rb.linearVelocity.z;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Sqrt(horizontalSpeedSqr) / playerSpeed);
        if (normalizedSpeed < 0.01f) normalizedSpeed = 0f;
        return normalizedSpeed;
    }

    // Animation updates (original Update logic)
    void UpdateAnimations()
    {
        animator.SetBool("onGround", onGround);
        if (onGround)
            animator.SetBool("isJumping", false);

        bool isWalking = cachedMoveDirection != Vector3.zero && !(currentState is ZeroGMovementState);
        animator.SetBool("isWalking", isWalking);

        float normalizedSpeed = NormalizeVelocity();
        animator.SetFloat("Speed", normalizedSpeed);
    }

    // Collision reset – original resets air jumps on ANY jumpable collision, even mid‑air
    void OnCollisionEnter(Collision collision)
    {
        if ((jumpable.value & (1 << collision.gameObject.layer)) > 0)
            jumpAbility.ResetOnGround();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            var pickUp = other.GetComponent<PickUpDefault>();
            if (pickUp != null)
            {
                pickUp.onPickup();
                playerPoints += pickUp.points;
                OnScoreChanged?.Invoke(playerPoints);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(10);
            else
            {
                ShowDeathScreen();
                OnPlayerDeath?.Invoke();
            }
        }
    }

    // Health & UI
    private void OnHealthDeath()
    {
        ShowDeathScreen();
        OnPlayerDeath?.Invoke();
    }

    public void ShowWinScreen() => winUI.gameObject.SetActive(true);
    public void ShowDeathScreen() => deathUI.gameObject.SetActive(true);

    // Unity lifecycle
    void FixedUpdate()
    {
        checkGround();
        currentState.FixedTick(this);
        HandleRotation();
    }

    void Update()
    {
        currentState.Tick(this);
        UpdateAnimations();
    }
}