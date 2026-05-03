

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerControllerWithHealth : MonoBehaviour
{
   /*Variables*/
    [Header("Look & Rotation")]
    [SerializeField] private float rotationSpeed = 10f; // Move in cached direction, not raw mouse delta, to avoid input blips
    /*Health*/
    private PlayerHealth playerHealth; //Ref to player health component

    /*Physics*/
    private Rigidbody rb; //Ref to rigidbody

    /*UI*/
    //[SerializeField] private TextMeshProUGUI countText; //Referance to Score UI element
    [SerializeField] private TextMeshProUGUI winUI;//Ref to Win UI 
    [SerializeField] private TextMeshProUGUI deathUI;//Ref to Death UI 
    [SerializeField] private GameObject HUD;


    /*Score*/
    private int playerPoints; //Storing score per player

    /*Sound*/
    [SerializeField] private AudioSource audioSource;//AudioSource Component Ref
    [SerializeField] private AudioClip[] jumpSFX;//List of jumpSFX
    [SerializeField] private AudioClip[] airJumpSFX;//List of airjumpSFX

    /*  Events  */
    public delegate void ScoreChangedDelegate(int newScore);
    public event ScoreChangedDelegate OnScoreChanged; //Score Changed event for efficiency
    public delegate void DeathDelegate();
    public event DeathDelegate OnPlayerDeath; //Score Changed event for efficiency

    /*Movement*/

    [SerializeField] private MovementMode defaultMode = MovementMode.AccelerationBased;
    [SerializeField] private MovementMode moveMode;
    private float moveX; //X Movement variable
    private float moveY; //Y Movement variable
    private float moveZ;
    [SerializeField] private float playerSpeed = 5.0f;//Speed of character movement Default 5
    [SerializeField] private float zgAcceleration = 1.0f;
    [SerializeField] private float acceleration = 10.0f;
    [SerializeField] private float deceleration = 15.0f;
    [SerializeField] private float zgDeceleration = 0.0f;
    [SerializeField] private float maxWalkableSlopeAngle = 45f;

    
    /*Jump*/
    [SerializeField] private int maxInAirjumps = 1;//Extra jumps in air
    private int jumpCharges;//air jumps left
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airJumpForce = 8.0f;
    private CapsuleCollider capsule;
    private bool onGround = false;
    private Vector3 groundNormal = Vector3.up;
    [SerializeField] private bool canJump = true;//Default player can jump //maybe also add inair jump only
    private LayerMask jumpable;//Jumpable layer mask

    /*Look*/
    [SerializeField] private Transform cameraPivot; // assign your camera (or empty parent)
    private float currentPitch;
    private Vector3 cachedMoveDirection;
    private Vector2 lookInput;

    private PlayerInput playerInput;

    /*Tie in Animations*/
    public Animator animator;



    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        HUD.SetActive(true);
        //use getcomponent to retrieve the animator
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); //Uses GetComponent to set rd to Rigidbody component
        //setPlayerScore();// update UI
        jumpable = LayerMask.GetMask("Jumpable");//get layermask
        playerHealth = GetComponent<PlayerHealth>(); //Get PlayerHealth component
        capsule = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;//Lock cursor
        Cursor.visible = false;


        // Subscribe to player death event from health component
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath.AddListener(OnHealthDeath);
        }

        switch (moveMode)
        {
            case MovementMode.ZeroGrav:
                enterZeroG();
                break;
        }     
    }
    /*Input Events*/
    /*
    void OnLook(InputValue value)//On axis movement
    {
        lookInput = value.Get<Vector2>();//get input
    }
    */
    public void OnInteract(InputValue inputValue)
    {
        Debug.Log($"[PlayerControllerWithHealth] OnInteract called! isPressed: {inputValue.isPressed}");
    }
    public float getPlayerSpeed()
    {
        return playerSpeed;
    }
    public void setPlayerSpeed(float newSpeed)
    {
        playerSpeed = newSpeed;
        return;
    }
    void OnMove(InputValue movementValue)//On any movement?
    {
        Vector2 movementVector = movementValue.Get<Vector2>();//Getting movement direction from movementValue param, and set it to Vector2 movementVector; (x,y)
        moveX = movementVector.x; // extract x from movementVector (x,y) make avalable to rest of code
        moveY = movementVector.y; // extract y from movementVector (y,x) make avalable to rest of code 
    }
    void OnJump(InputValue jumpValue)//Jump input
    {
        if (jumpValue.isPressed)
        {
            jump();
            //Debug.Log("Jump Pressed");
        }
    }
    void enterZeroG()
    {
        // 🔑 Zero-G setup
        rb.useGravity = false;
        rb.linearDamping = 0f; // no automatic slowing
    }
    void exitZeroG()
    {
        rb.useGravity = true;
        rb.linearDamping = 0f;

        // prevent weird float carryover when re-entering gravity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }
    public MovementMode GetDefaultMode()
    {
        return defaultMode;
    }
    public void SetMovementMode(MovementMode newMode)
    {
        if (moveMode == newMode) return;

        // Exit current mode
        switch (moveMode)
        {
            case MovementMode.ZeroGrav:
                exitZeroG();
                break;
        }

        moveMode = newMode;

        // Enter new mode
        switch (moveMode)
        {
            case MovementMode.ZeroGrav:
                enterZeroG();
                break;
        }
    }
    /*Movement*/
    void jump()//call on input to jump
    {
        switch(moveMode)
        {
            case MovementMode.ZeroGrav:

                break;
            default:
                //Ground jump logic
                if (onGround && canJump)
                {
                    //resetAirJumps();//reset airjumps on ground
                    handleJump(jumpForce);//Physics
                    animator.SetBool("isJumping", true);
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                    playRandomSFX(jumpSFX);//Sound
                    Debug.Log("Jump");
                }
                //Air jump logic
                if (!onGround && canJump && jumpCharges > 0)
                {
                    --jumpCharges;
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                    handleJump(airJumpForce);//Physics
                    playRandomSFX(airJumpSFX);//Sound
                    Debug.Log("DoubleJump");

                }
                    //check if on ground and has jumpcharge  max 3  3jump charges
                break;
                
        //
        }

}
    void checkGround()//raycast bellow player check for ground
    {
        float radius = capsule.radius * 0.95f; // slightly smaller to avoid wall hits
        
        float castDistance = (capsule.height * 0.5f) - capsule.radius + 0.1f;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        onGround = Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out RaycastHit hit,
            castDistance,
            jumpable
        );

        groundNormal = onGround ? hit.normal : Vector3.up;

        Debug.DrawRay(origin, Vector3.down * castDistance, onGround ? Color.green : Color.red);
    }

    void handleJump(float jumpHight)//Takes in jump hight
    {
        if (rb.linearVelocity.y < 0)//resets verical velocity if player is falling, keeps upward velocity
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
        rb.AddForce(Vector3.up * jumpHight, ForceMode.Impulse); //impulse force
    }

    void resetAirJumps()//Reset air jumps
    {
        jumpCharges = maxInAirjumps;
        //Debug.Log("RESET AIR JUMP");
    }

    void setmaxInAirjumps(int jumps) //not used currently
    {
        maxInAirjumps = jumps;
    }

    /*  Handle players score  */
    /*public int getPlayerScore()
    {
        return playerPoints;
    }
    void setPlayerScore()//Displays player's score on UI
    {
        //countText.text = "Score: " + playerPoints.ToString();
    }*/
    public void ShowWinScreen()
    {
        winUI.gameObject.SetActive(true);
    }
    public void ShowDeathScreen()
    {
        deathUI.gameObject.SetActive(true);
    }

    /*  Handle player health and death  */
    private void OnHealthDeath()/// Called when PlayerHealth component detects death
    {
        ShowDeathScreen();
        OnPlayerDeath?.Invoke();
    }
    
    /* Sound Helper */
    private void playRandomSFX(AudioClip[] soundList)
    {
        int randomIndex = Random.Range(0, soundList.Length);
        if (soundList.Length > 0)//make sure there is a sound to play
        {
            audioSource.PlayOneShot(soundList[randomIndex]);
        }
    }
    /*  Collision detection  */
    void OnTriggerEnter(Collider other)//execute once on trigger
    {
        if(other.gameObject.CompareTag("PickUp"))//If pickup collected
        {
            PickUpDefault pickUp = other.gameObject.GetComponent<PickUpDefault>();//Get PickUp
            pickUp.onPickup();//call pickUp's onPickup function
            playerPoints += pickUp.points;//check pickups point value stored in PickUpDefault script
            print("PlayerPoints: " + playerPoints);//Debug
            //setPlayerScore();// update UI
            OnScoreChanged?.Invoke(playerPoints);//Notify listeners for score update passing playerPoints

        }
        Debug.Log("test");
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy touched");
            // Deal damage to player through health system
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10); // 10 damage per contact
            }
            else
            {
                //if no health component
                ShowDeathScreen();
                OnPlayerDeath?.Invoke();
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if ((jumpable.value & (1 << collision.gameObject.layer)) > 0) //If collision layer == jumpable layer (only reset jumps on jumpable surface)
        {
            resetAirJumps();
        }
    }
    void PreventWallSticking(ref Vector3 velocity)
    {
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontal.sqrMagnitude < 0.0001f) return;

        // Cast from capsule center, not the root pivot (feet on animated rigs).
        Vector3 origin = transform.TransformPoint(capsule.center);
        float radius   = capsule.radius * 0.95f;
        float distance = 0.1f;

        int mask = ~LayerMask.GetMask("Player"); // make sure astronaut root + children are on "Player"

        if (Physics.SphereCast(origin, radius, horizontal.normalized,
                            out RaycastHit hit, distance, mask,
                            QueryTriggerInteraction.Ignore))
        {
            // Floors / tops of boxes are not walls — never project against them.
            if (hit.normal.y > 0.5f) return;

            // Dynamic rigidbodies: let the physics solver handle push/contact instead of
            // overriding linearVelocity into them. Zero only the inward component so we
            // don't keep ramming and don't glue to the side either.
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                float into = Vector3.Dot(horizontal, -hit.normal);
                if (into > 0f) horizontal += hit.normal * into;  // cancel inward push
                return;
            }

            if (Vector3.Dot(horizontal, hit.normal) < 0f)
                horizontal = Vector3.ProjectOnPlane(horizontal, hit.normal);
        }

        velocity.x = horizontal.x;
        velocity.z = horizontal.z;
    }
    /*Update*/
    void FixedUpdate()//Fixed interval update ensures physics is consistant regaurdless of framerate
    {

        Vector3 movement = cachedMoveDirection;
        
        switch (moveMode)
        {
        case MovementMode.AccelerationBased:

            if (movement.sqrMagnitude > 1f)
                movement.Normalize();

            Vector3 targetVelocity = movement * playerSpeed;
            // --- Begin slope handling ---
            // Slope handling logic 05-02-26 - Claude Assisted -- David Haddad
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool ascending = rb.linearVelocity.y > 0.1f; // jumping or otherwise being pushed up — don't clamp
            bool onWalkableSlope = onGround && !ascending && slopeAngle > 0.1f && slopeAngle <= maxWalkableSlopeAngle;

            if (onWalkableSlope)
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
            else
                targetVelocity.y = rb.linearVelocity.y;
            // --- End slope handling ---

            float accelRate = (movement.sqrMagnitude > 0.01f) ? acceleration : deceleration;
            float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);

            Vector3 velocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t);

            // Clamp horizontal speed
            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
            horizontal = Vector3.ClampMagnitude(horizontal, playerSpeed);

            velocity = new Vector3(horizontal.x, velocity.y, horizontal.z);

            // Zero-friction anti-slide: hold position on walkable slopes when no input. 05-02-26 -- David Haddad
            if (onWalkableSlope && cachedMoveDirection.sqrMagnitude < 0.01f)
                velocity = Vector3.zero;

            PreventWallSticking(ref velocity);

            rb.linearVelocity = velocity;

            break;
            case MovementMode.ZeroGrav:
                // Target velocity in full 3D
                Vector3 targetzVelocity = movement * playerSpeed;

                float accelzRate = (movement.sqrMagnitude > 0.01f) ? zgAcceleration : zgDeceleration;

                float tz = 1f - Mathf.Exp(-accelzRate * Time.fixedDeltaTime);

                Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetzVelocity, tz);

                // Clamp total speed (not just horizontal!)
                newVelocity = Vector3.ClampMagnitude(newVelocity, playerSpeed);

                rb.linearVelocity = newVelocity;
                break;
        }

        // If we have input, rotate the player to face the movement direction
        if (cachedMoveDirection != Vector3.zero && moveMode != MovementMode.ZeroGrav)
        {
            // Determine the rotation we need to look in the direction of movement
            Quaternion targetRotation = Quaternion.LookRotation(cachedMoveDirection);
            
            // Smoothly rotate from our current rotation to the target rotation
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
        
        checkGround();//check if player is on the ground
    }
    float NormalizeVelocity()
    {
        // Most efficient — no Vector3 allocation, single calculation
        float horizontalSpeedSqr = rb.linearVelocity.x * rb.linearVelocity.x + rb.linearVelocity.z * rb.linearVelocity.z;
        float normalizedSpeed = Mathf.Clamp01(Mathf.Sqrt(horizontalSpeedSqr) / playerSpeed);
        if (normalizedSpeed < 0.01f)
        normalizedSpeed = 0f;
        return normalizedSpeed;
    }
    void Update()
    {
        //look();
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
        animator.SetBool("onGround", onGround);
        animator.SetBool("onGround", onGround);
        if (onGround==true)//check if player is on the ground
        {
            animator.SetBool("isJumping", false);
        }
        switch (moveMode)
        {
            case MovementMode.ZeroGrav:
                // Camera-relative movement
                Vector3 up = cameraPivot.up;

                cachedMoveDirection = forward * moveY + right * moveX + up * moveZ;

                if (cachedMoveDirection.sqrMagnitude > 1f)
                    cachedMoveDirection.Normalize();
                break;

            default://everything else
                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();

                cachedMoveDirection = forward * moveY + right * moveX;
                if (cachedMoveDirection == Vector3.zero)
                    animator.SetBool("isWalking", false);
                else
                    animator.SetBool("isWalking", true);
                    float normalizedVelocity = NormalizeVelocity();
                    animator.SetFloat("Speed", normalizedVelocity);
                    //Debug.Log("S: " + rb.linearVelocity.magnitude + "N: "+ normalizedVelocity);
                break;
        }



    }
}