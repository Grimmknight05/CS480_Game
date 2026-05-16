using UnityEngine;

public class JumpAbility : MovementAbility
{
    //General
    private float groundJumpForce;
    //Doublejump
    private int maxAirJumps;
    private int airJumpsLeft;
    private float airJumpForce;
    private AudioClip[] groundJumpClips;
    private AudioClip[] airJumpClips;
    private AudioSource audioSource;
    //Ground Jump cooldown
    [SerializeField] private float groundResetDelay = 0.2f;   // delay after landing until jumps reset
    private float groundCooldownTimer = 0f;      // groundCooldowntimer
    private bool wasGrounded;
    //In between jump cooldown
    [SerializeField] private float jumpCooldown = 0.1f;       // prevents multiple jumps from spamming
    private float lastJumpTime = -999f;     // last time any jump was performed
    
    //Variable height jump on hold
    [SerializeField] private float jumpHoldMaxTime = 0.5f;    // how long you can hold for extra height
    [SerializeField] private float jumpHoldVelocityPerFrame  = 0.1f; // force per second while holding
    [SerializeField] private float maxJumpVelocity = 15f; // Maximum jump Velocity cap
    private float jumpHoldTimer = 0f;
    private bool isJumpHolding = false;
    public float LastJumpTime => lastJumpTime;
    public bool IsJumpHolding => isJumpHolding;
    //MainCall
    public JumpAbility(int maxAirJumps, float groundForce, float airForce,
                       AudioClip[] groundClips, AudioClip[] airClips, AudioSource src)//Might be worth while to implement a config SO that can be passed into player and hold all these settings
    {
        this.maxAirJumps = maxAirJumps;
        this.groundJumpForce = groundForce;
        this.airJumpForce = airForce;
        this.groundJumpClips = groundClips;
        this.airJumpClips = airClips;
        this.audioSource = src;
        IsEnabled = true;
    }

    public override void Enter(PlayerControllerRefactored player)
    {
        airJumpsLeft = maxAirJumps;
        groundCooldownTimer = 0f;
        lastJumpTime = -999f;
        wasGrounded = player.OnGround;
    }

    public override bool CanExecute(PlayerControllerRefactored player)
    {
        if (!IsEnabled) return false;

        // Check jump cooldown (prevents spamming)
        if (Time.time - lastJumpTime < jumpCooldown)
            return false;

        if (player.IsWalkableGround)
        {
            // On ground: can jump only if ground cooldown finished AND CanJump
            return groundCooldownTimer <= 0f && player.CanJump;
        }
        else
        {
            // In air: can jump only if we have air jumps left
            return airJumpsLeft > 0;
        }
    }

    public override void Execute(PlayerControllerRefactored player)
    {
        // Record jump time for cooldown
        lastJumpTime = Time.time;

        Rigidbody rb = player.rb;
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        bool isGroundJump = player.OnGround;
        if (isGroundJump)
        {
            rb.AddForce(Vector3.up * groundJumpForce, ForceMode.Impulse);
            PlayRandomClip(groundJumpClips);
            player.Animator.SetBool("isJumping", true);
            player.Animator.ResetTrigger("Jump");
            player.Animator.SetTrigger("Jump");
            isJumpHolding = true;
            jumpHoldTimer = 0f;
            Debug.Log("Jump executed, holding started");
        }
        else
        {
            // Air jump: consume one
            airJumpsLeft--;
            rb.AddForce(Vector3.up * airJumpForce, ForceMode.Impulse);
            PlayRandomClip(airJumpClips);
            player.Animator.ResetTrigger("Jump");
            player.Animator.SetTrigger("Jump");
        }
    }
    public void OnJumpHeld(PlayerControllerRefactored player)
    {
        if (!isJumpHolding) return;
        
        jumpHoldTimer += Time.fixedDeltaTime;
        if (jumpHoldTimer < jumpHoldMaxTime)
        {
            // Add force, but don't exceed max velocity
            float currentY = player.rb.linearVelocity.y;
            if (currentY < maxJumpVelocity)
            {
                float remaining = maxJumpVelocity - currentY;
                float add = Mathf.Min(jumpHoldVelocityPerFrame, remaining);
                player.rb.AddForce(Vector3.up * add, ForceMode.VelocityChange);
            }
        }
        else
        {
            isJumpHolding = false;
        }
    }
    // Call this when the jump button is released
    public void OnJumpReleased()
    {
        isJumpHolding = false;
    }
    public void UpdateAbility(PlayerControllerRefactored player)
    {
        // Detect landing
        bool onGround = player.OnGround;
        if (onGround && !wasGrounded)
        {
            // Just landed – start recharge delay (blocks ground jumps until it finishes)
            groundCooldownTimer = groundResetDelay;
        }
        wasGrounded = onGround;

        // Update ground cooldown
        if (groundCooldownTimer > 0)
        {
            groundCooldownTimer -= Time.deltaTime;
            if (groundCooldownTimer <= 0f)
            {
                // Recharge complete: restore all air jumps
                airJumpsLeft = maxAirJumps;
                // ground jump becomes available automatically in CanExecute
            }
        }
    }
    //Effect Helpers
    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        int idx = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[idx]);
    }
}