

using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI countText; //Referance to Score UI element
    [SerializeField] private TextMeshProUGUI winUI;//Ref to Win UI 
    [SerializeField] private TextMeshProUGUI deathUI;//Ref to Death UI 
    


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

    
    /*Jump*/
    [SerializeField] private int maxInAirjumps = 1;//Extra jumps in air
    private int jumpCharges;//air jumps left
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airJumpForce = 8.0f;
    [SerializeField] private float jumpRayDistance = 0.6f; //Raydistance important to account for player height
    private bool onGround = false;
    [SerializeField] private bool canJump = true;//Default player can jump //maybe also add inair jump only
    private LayerMask jumpable;//Jumpable layer mask

    /*Look*/
    [SerializeField] private Transform cameraPivot; // assign your camera (or empty parent)
    private float currentPitch;
    private Vector3 cachedMoveDirection;
    private Vector2 lookInput;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); //Uses GetComponent to set rd to Rigidbody component
        setPlayerScore();// update UI
        jumpable = LayerMask.GetMask("Jumpable");//get layermask
        playerHealth = GetComponent<PlayerHealth>(); //Get PlayerHealth component

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
                    playRandomSFX(jumpSFX);//Sound
                    Debug.Log("Jump");
                }
                //Air jump logic
                if (!onGround && canJump && jumpCharges > 0)
                {
                    --jumpCharges;
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
        onGround = Physics.Raycast(transform.position, Vector3.down, jumpRayDistance, jumpable); //(origon position, direction, length, Layer(like you cant jump on water) difined in gameobjects
        //Debug.Log("onGround: " + onGround);
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
    public int getPlayerScore()
    {
        return playerPoints;
    }
    void setPlayerScore()//Displays player's score on UI
    {
        countText.text = "Score: " + playerPoints.ToString();
    }
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
            setPlayerScore();// update UI
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
    /*Update*/
    void FixedUpdate()//Fixed interval update ensures physics is consistant regaurdless of framerate
    {

        Vector3 movement = cachedMoveDirection;
        
        switch (moveMode)
        {
            case MovementMode.ForceBased:
                //Construct movement vector3
                //Vector3 movement = new Vector3(moveX, 0.0f, moveY); // Asign new Vector3(x,z,y) with moveX and moveY input, and no z input

                //Add force to player in cormovement
                /*rb.AddForce(movement * playerSpeed);//multiply movement Vector3 by playerSpeed varible
                break;
            case MovementMode.VelocityBased:
                Vector3 velocity = new Vector3(moveX * playerSpeed, rb.linearVelocity.y, moveY * playerSpeed);
                rb.linearVelocity = velocity;
                break;*/
            case MovementMode.AccelerationBased:

                //Vector3 aMovement = new Vector3(moveX, 0f, moveY); // Asign new Vector3(x,z,y) with moveX and moveY input, and no z input

                if (movement.sqrMagnitude > 1f) //Normalize to limit faster diagonal movement
                    movement.Normalize();



                Vector3 targetVelocity = movement * playerSpeed; //find target velocity
                targetVelocity.y = rb.linearVelocity.y;//keep y velocity uneffected

                float accelRate = (movement.sqrMagnitude > 0.01f) ? acceleration : deceleration;//Calculate acceleration rate

                float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);//Smooth t

                Vector3 avelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t); // Lerp velocity

                // Clamp horizontal speed
                Vector3 horizontal = new Vector3(avelocity.x, 0, avelocity.z);
                horizontal = Vector3.ClampMagnitude(horizontal, playerSpeed);

                rb.linearVelocity = new Vector3(horizontal.x, avelocity.y, horizontal.z); //set velocity
                
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
    void Update()
    {
        //look();
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;
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
                break;
        }



    }
}
