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
    private bool onSteepSlope;

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
    private RaycastHit groundHit;      // stores the most recent ground hit
    private bool hasGroundHit = false; // whether groundHit is valid this frame
    public JumpAbility jumpAbility;
    private Queue<ICommand> inputQueue = new Queue<ICommand>();
    
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

    public void SetMovementMode(MovementMode mode)
    {
        if (mode == MovementMode.ZeroGrav)
            SetMovementState(new ZeroGMovementState());
        else
            SetMovementState(new GroundedMovementState());
    }
    public void QueueCommand(ICommand command)
    {
        inputQueue.Enqueue(command);
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
        QueueCommand(new JumpCommand());
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log($"[PlayerController] OnInteract called, isPressed: {value.isPressed}");
    }
    void checkGround()
    {
        if (capsule == null) return;

        float radius = capsule.radius * 0.95f;
        float castDistance = (capsule.height * 0.5f) - capsule.radius + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        bool newGrounded = Physics.SphereCast(origin, radius, Vector3.down, out groundHit, castDistance, jumpable);
        hasGroundHit = newGrounded;
        if (newGrounded)
            groundNormal = groundHit.normal;
        else
            groundNormal = Vector3.up;

        // Forward-down ray for small steps
        if (!newGrounded && rb.linearVelocity.y <= 0.2f)
        {
            Vector3 moveDir = cachedMoveDirection.sqrMagnitude > 0.01f ? cachedMoveDirection.normalized : transform.forward;
            Vector3 kneeOrigin = transform.position + Vector3.up * 0.5f;
            float stepDistance = 0.4f;
            if (Physics.Raycast(kneeOrigin, moveDir, out RaycastHit stepWall, stepDistance, jumpable))
            {
                if (stepWall.normal.y < 0.2f)
                {
                    Vector3 aboveStep = kneeOrigin + Vector3.up * 0.3f + moveDir * stepWall.distance;
                    if (Physics.Raycast(aboveStep, Vector3.down, out RaycastHit stepTop, 0.5f, jumpable))
                    {
                        float stepHeight = stepTop.point.y - transform.position.y;
                        if (stepHeight > 0.05f && stepHeight < 0.35f)
                        {
                            newGrounded = true;
                            groundNormal = stepTop.normal;
                            groundHit = stepTop;
                            hasGroundHit = true;
                        }
                    }
                }
            }
        }

        onGround = newGrounded;

        //if (newGrounded && !wasGrounded)
        //    jumpAbility.ResetOnGround();

        wasGrounded = newGrounded;

        Debug.DrawRay(origin, Vector3.down * castDistance, onGround ? Color.green : Color.red);
        Debug.DrawRay(origin + Vector3.down * castDistance, Vector3.up * 0.2f, Color.yellow);
    }

    private void HandleStepUp()
    {
        if (!onGround || rb.linearVelocity.y > 0.1f) return;

        float stepHeight = 0.3f;
        float stepCheckDistance = 0.4f;

        Vector3 moveDir = cachedMoveDirection;
        if (moveDir.sqrMagnitude < 0.01f) return;

        Vector3 origin = transform.position + Vector3.up * 0.1f; // foot level
        if (!Physics.Raycast(origin, moveDir.normalized, out RaycastHit wallHit, stepCheckDistance, jumpable))
            return;

        if (wallHit.normal.y > 0.2f) return; // not a vertical wall

        Vector3 aboveOrigin = origin + Vector3.up * stepHeight + moveDir.normalized * wallHit.distance;
        if (Physics.Raycast(aboveOrigin, Vector3.down, out RaycastHit stepHit, stepHeight + 0.2f, jumpable))
        {
            float stepHeightActual = stepHit.point.y - transform.position.y;
            if (stepHeightActual > 0.05f && stepHeightActual <= stepHeight)
            {
                Vector3 newPos = transform.position;
                newPos.y = stepHit.point.y + 0.05f;
                transform.position = newPos;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }
    }

    public void PreventWallSticking(ref Vector3 velocity)
    {
        if (rb.linearVelocity.y > 0.2f) return; // allow upward movement

        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude < 0.0001f) return;

        Vector3 origin = transform.TransformPoint(capsule.center);
        float radius = capsule.radius * 0.95f;
        float distance = 0.1f;
        int mask = ~LayerMask.GetMask("Player");

        if (Physics.SphereCast(origin, radius, horizontal.normalized, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(hit.point, hit.normal, Color.blue, 0.5f);
            if (hit.normal.y > 0.5f) return;

            float lipThreshold = 0.2f;
            if (hit.normal.y > lipThreshold && horizontal.magnitude < 2f)
                return;   // allow gentle step up

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
    public void HandleGroundMovement()
    {
        Vector3 movement = cachedMoveDirection;
        float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        bool ascending = rb.linearVelocity.y > 0.1f;
        bool onWalkableSlope = onGround && !ascending && slopeAngle > 0.1f && slopeAngle <= maxWalkableSlopeAngle;
        bool isSmallLip = false;
        if (onGround && slopeAngle > maxWalkableSlopeAngle)
        {
            float verticalDiff = transform.position.y - groundHit.point.y;
            

            if (verticalDiff < 0.3f && Mathf.Abs(Vector3.Dot(cachedMoveDirection, groundNormal)) > 0.7f)
            {
                isSmallLip = true;
            }
        }
        onSteepSlope = onGround && !isSmallLip && slopeAngle > maxWalkableSlopeAngle;
        if (onSteepSlope && !isSmallLip)
        {
            // Cancel player input
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            float slideAcceleration = 25f; // tune this for desired slide speed
            rb.AddForce(slideDirection * slideAcceleration, ForceMode.Acceleration);
            
            rb.linearDamping = 0.5f;
            return;
        }
        else
        {
            // Reset damping when not on steep slope
            rb.linearDamping = 0f;
        }

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Vector3 targetVelocity = movement * playerSpeed;
        HandleStepUp();
        


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

    public void HandleRotation()
    {
        if (currentState is ZeroGMovementState) 
        
        return;
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

    float NormalizeVelocity()
    {
        float horizontalSpeedSqr = rb.linearVelocity.x * rb.linearVelocity.x + rb.linearVelocity.z * rb.linearVelocity.z;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Sqrt(horizontalSpeedSqr) / playerSpeed);
        if (normalizedSpeed < 0.01f) normalizedSpeed = 0f;
        return normalizedSpeed;
    }

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
    void OnCollisionEnter(Collision collision)
    {
        //if ((jumpable.value & (1 << collision.gameObject.layer)) > 0)
            //jumpAbility.ResetOnGround();
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
    private void ProcessCommandQueue()
    {
        while (inputQueue.Count > 0)
        {
            ICommand cmd = inputQueue.Peek();
            // Remove if expired
            if (Time.time > cmd.ExpiryTime)
            {
                inputQueue.Dequeue();
                continue;
            }
            // Attempt execute
            if (cmd.CanExecute(this))
            {
                cmd.Execute(this);
                inputQueue.Dequeue(); 
                break; // Only one command per frame
            }
            else
            {
                // Cannot execute yet, keep it in queue (maybe move to back? Usually keep order)
                break;
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
        // Process input queue
        jumpAbility.UpdateAbility(this);
        ProcessCommandQueue();
        
    }
}