using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerRefactored : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    public Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerHealth playerHealth;
    private PlayerInput playerInput;

    [Header("Movement Settings")]
    [SerializeField, Tooltip("Starting movement mode")] private MovementMode initialMode = MovementMode.AccelerationBased;
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField, Range(0f, 90f)] private float maxWalkableSlopeAngle = 45f;

    [Header("Zero Gravity")]
    [SerializeField] private float zgAcceleration = 1f;
    [SerializeField] private float zgDeceleration = 0f;

    [Header("Jump")]
    [SerializeField] private int maxInAirjumps = 1;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float airJumpForce = 8f;
    [SerializeField] private bool canJump = true;
    [SerializeField] private AudioClip[] jumpSFX;
    [SerializeField] private AudioClip[] airJumpSFX;

    [Header("Ground Snapping")]
    [SerializeField] private float groundSnapMaxDistance = 1.0f;
    [SerializeField] private float snapBlockDurationAfterJump = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI winUI;
    [SerializeField] private GameObject HUD;

    [Header("Dialogue")]
    [SerializeField] private DialogueEventChannelSO dialogueStartChannel;
    [SerializeField] private DialogueEndedChannelSO dialogueEndedChannel;

    // Movement state
    private MovementState currentState;
    private Vector3 cachedMoveDirection;
    private float moveX, moveY, moveZ;

    // Ground state
    private bool onGround;
    private bool wasGrounded;
    private Vector3 groundNormal = Vector3.up;
    private RaycastHit groundHit;
    private bool hasGroundHit;
    public bool IsWalkableGround { get; private set; }
    private LayerMask jumpable;

    // Input & commands
    private Queue<ICommand> inputQueue = new Queue<ICommand>();
    private InputAction jumpAction;
    public bool InputEnabled { get; private set; } = true;
    private MovementMode previousModeBeforeDialogue;

    // Abilities
    public JumpAbility jumpAbility;

    // Events
    public delegate void ScoreChangedDelegate(int newScore);
    public event ScoreChangedDelegate OnScoreChanged;
    public delegate void DeathDelegate();
    public event DeathDelegate OnPlayerDeath;

    // Public properties
    public bool OnGround => onGround;
    public Vector3 GroundNormal => groundNormal;
    public Animator Animator => animator;
    public float PlayerSpeed => playerSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float ZGAcceleration => zgAcceleration;
    public float ZGDeceleration => zgDeceleration;
    public float MaxWalkableSlopeAngle => maxWalkableSlopeAngle;
    public bool CanJump => canJump;

    #region Unity Lifecycle
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

        SetMovementState(initialMode == MovementMode.ZeroGrav
            ? new ZeroGMovementState()
            : new GroundedMovementState());
    }

    void OnEnable()
    {
        var inputActions = GetComponent<PlayerInput>().actions;
        jumpAction = inputActions.FindAction("Jump");
        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;

        if (dialogueStartChannel != null) dialogueStartChannel.OnRaised += HandleDialogueStart;
        if (dialogueEndedChannel != null) dialogueEndedChannel.OnRaised += HandleDialogueEnded;
    }

    void OnDisable()
    {
        jumpAction.started -= OnJumpStarted;
        jumpAction.canceled -= OnJumpCanceled;

        if (dialogueStartChannel != null) dialogueStartChannel.OnRaised -= HandleDialogueStart;
        if (dialogueEndedChannel != null) dialogueEndedChannel.OnRaised -= HandleDialogueEnded;
    }

    void FixedUpdate()
    {
        checkGround();
        SnapToGroundIfClose();
        currentState.FixedTick(this);
        jumpAbility?.OnJumpHeld(this);
        HandleRotation();
    }

    void Update()
    {
        currentState.Tick(this);
        UpdateAnimations();
        jumpAbility.UpdateAbility(this);
        ProcessCommandQueue();
    }
    #endregion

    #region Input & Dialogue
    void OnMove(InputValue value)
    {
        if (!InputEnabled)
        {
            moveX = moveY = 0f;
            return;
        }
        Vector2 v = value.Get<Vector2>();
        moveX = v.x;
        moveY = v.y;
    }

    public void OnInteract(InputValue value)
    {
        if (!InputEnabled) return;
        Debug.Log($"[PlayerController] OnInteract called, isPressed: {value.isPressed}");
    }

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        if (!InputEnabled) return;
        QueueCommand(new JumpCommand());
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        jumpAbility.OnJumpReleased();
    }

    public void QueueCommand(ICommand command)
    {
        inputQueue.Enqueue(command);
    }

    private void ProcessCommandQueue()
    {
        while (inputQueue.Count > 0)
        {
            ICommand cmd = inputQueue.Peek();
            if (Time.time > cmd.ExpiryTime)
            {
                inputQueue.Dequeue();
                continue;
            }
            if (cmd.CanExecute(this))
            {
                cmd.Execute(this);
                inputQueue.Dequeue();
                break;
            }
            break;
        }
    }

    private void HandleDialogueStart(DialogueSO _)
    {
        previousModeBeforeDialogue = (currentState is ZeroGMovementState) ? MovementMode.ZeroGrav : MovementMode.AccelerationBased;
        InputEnabled = false;
        moveX = moveY = moveZ = 0f;
        SetMovementState(new DialogueMovementState());
    }

    private void HandleDialogueEnded()
    {
        InputEnabled = true;
        SetMovementMode(previousModeBeforeDialogue);
    }
    #endregion

    #region Movement State Management
    public void SetMovementState(MovementState newState)
    {
        if (currentState == newState) return;
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void SetMovementMode(MovementMode mode)
    {
        SetMovementState(mode == MovementMode.ZeroGrav ? new ZeroGMovementState() : new GroundedMovementState());
    }

    public void UpdateGroundMovementInput()
    {
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        forward.y = right.y = 0f;
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
    #endregion

    #region Ground & Collision
    private void checkGround()
    {
        if (capsule == null) return;

        float radius = capsule.radius * 0.95f;
        float castDistance = (capsule.height * 0.5f) - capsule.radius + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        bool newGrounded = Physics.SphereCast(origin, radius, Vector3.down, out groundHit, castDistance, jumpable);
        hasGroundHit = newGrounded;
        groundNormal = newGrounded ? groundHit.normal : Vector3.up;

        // Optional step-up ray (commented out if not used)
        // if (!newGrounded && rb.linearVelocity.y <= 0.2f) { ... }

        IsWalkableGround = newGrounded && Vector3.Angle(Vector3.up, groundNormal) <= maxWalkableSlopeAngle;
        onGround = newGrounded;
        wasGrounded = newGrounded;
    }

    private void SnapToGroundIfClose()
    {
        if (jumpAbility.IsJumpHolding || (Time.time - jumpAbility.LastJumpTime < snapBlockDurationAfterJump))
            return;

        float radius = capsule.radius * 0.95f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundSnapMaxDistance, jumpable))
        {
            float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (slopeAngle <= maxWalkableSlopeAngle)
            {
                float targetY = hit.point.y + (capsule.height * 0.5f - capsule.radius);
                float deltaY = transform.position.y - targetY;

                if (deltaY > 0.01f)
                {
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                    if (rb.linearVelocity.y > 0f)
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                    onGround = true;
                    IsWalkableGround = true;
                    groundNormal = hit.normal;
                    groundHit = hit;
                    hasGroundHit = true;
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & jumpable) == 0) return;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(Vector3.up, contact.normal) > maxWalkableSlopeAngle)
                Debug.DrawRay(contact.point, contact.normal, Color.magenta, 0.5f);
        }
    }
    #endregion

    #region Movement Logic
    public void HandleGroundMovement()
    {
        // Steep slope sliding
        if (onGround && !IsWalkableGround)
        {
            bool isJumping = jumpAbility.IsJumpHolding || (Time.time - jumpAbility.LastJumpTime < snapBlockDurationAfterJump);
            if (!isJumping && rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            rb.AddForce(slideDirection * 25f, ForceMode.Acceleration);
            rb.linearDamping = 0.5f;
            return;
        }

        // Normal walkable ground
        if (onGround && IsWalkableGround)
        {
            rb.linearDamping = 0f;
            Vector3 movement = cachedMoveDirection;
            if (movement.sqrMagnitude > 1f) movement.Normalize();

            Vector3 targetVelocity = movement * playerSpeed;
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool ascending = rb.linearVelocity.y > 0.1f;
            bool onWalkableSlope = !ascending && slopeAngle > 0.1f;

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

            if (onWalkableSlope && cachedMoveDirection.sqrMagnitude < 0.01f)
                velocity = Vector3.zero;

            PreventWallSticking(ref velocity);
            rb.linearVelocity = velocity;
            return;
        }

        // Air movement
        HandleAirMovement();
    }

    public void HandleAirMovement()
    {
        Vector3 movement = cachedMoveDirection;
        if (movement.sqrMagnitude > 1f) movement.Normalize();

        Vector3 targetVelocity = movement * playerSpeed;
        float airAccel = acceleration * 0.5f;
        float t = 1f - Mathf.Exp(-airAccel * Time.fixedDeltaTime);
        Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);
        newVelocity = Vector3.ClampMagnitude(newVelocity, playerSpeed);
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;
    }

    public void HandleZeroGMovement()
    {
        Vector3 movement = cachedMoveDirection;
        if (movement.sqrMagnitude > 1f) movement.Normalize();

        Vector3 targetVelocity = movement * playerSpeed;
        float accelRate = (movement.sqrMagnitude > 0.01f) ? zgAcceleration : zgDeceleration;
        float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);
        Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);
        newVelocity = Vector3.ClampMagnitude(newVelocity, playerSpeed);
        rb.linearVelocity = newVelocity;
    }

    public void HandleRotation()
    {
        if (currentState is ZeroGMovementState) return;

        Vector3 dir = cachedMoveDirection;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    public void FaceCameraDirection()
    {
        Vector3 camForward = cameraPivot.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(camForward);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    private void PreventWallSticking(ref Vector3 velocity)
    {
        if (rb.linearVelocity.y > 0.2f) return;

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude < 0.0001f) return;

        Vector3 origin = transform.TransformPoint(capsule.center);
        float radius = capsule.radius * 0.95f;
        int mask = ~LayerMask.GetMask("Player");

        if (Physics.SphereCast(origin, radius, horizontal.normalized, out RaycastHit hit, 0.1f, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y > 0.5f) return;
            if (hit.normal.y > 0.2f && horizontal.magnitude < 2f) return;

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
    #endregion

    #region Animation & UI
    private float NormalizeVelocity()
    {
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        return Mathf.Clamp01(horizontalSpeed / playerSpeed);
    }

    private void UpdateAnimations()
    {
        animator.SetBool("onGround", onGround);
        if (onGround) animator.SetBool("isJumping", false);

        bool isWalking = cachedMoveDirection != Vector3.zero && !(currentState is ZeroGMovementState);
        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("Speed", NormalizeVelocity());
    }

    private void OnHealthDeath() => OnPlayerDeath?.Invoke();
    public void ShowWinScreen() => winUI.gameObject.SetActive(true);
    #endregion

    #region Events & Scoring
    void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enemy"))
        //{
        //    if (playerHealth != null) playerHealth.TakeDamage(10);
        //    else OnPlayerDeath?.Invoke();
        //}
    }
    #endregion
}