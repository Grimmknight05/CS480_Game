using UnityEngine;

public class JumpAbility : MovementAbility
{
    private int maxAirJumps;
    private int airJumpsLeft;
    private float groundJumpForce;
    private float airJumpForce;
    private AudioClip[] groundJumpClips;
    private AudioClip[] airJumpClips;
    private AudioSource audioSource;

    [SerializeField] private float groundResetDelay = 0.2f;   // delay after landing until jumps reset
    [SerializeField] private float jumpCooldown = 0.1f;       // prevents multiple jumps from spamming

    private float groundCooldown = 0f;      // >0 = cannot jump on ground (recharging)
    private float lastJumpTime = -999f;     // last time any jump was performed
    private bool wasGrounded;

    public JumpAbility(int maxAirJumps, float groundForce, float airForce,
                       AudioClip[] groundClips, AudioClip[] airClips, AudioSource src)
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
        groundCooldown = 0f;
        lastJumpTime = -999f;
        wasGrounded = player.OnGround;
    }

    public void UpdateAbility(PlayerControllerRefactored player)
    {
        // Detect landing
        bool onGround = player.OnGround;
        if (onGround && !wasGrounded)
        {
            // Just landed – start recharge delay (blocks ground jumps until it finishes)
            groundCooldown = groundResetDelay;
        }
        wasGrounded = onGround;

        // Update ground cooldown
        if (groundCooldown > 0)
        {
            groundCooldown -= Time.deltaTime;
            if (groundCooldown <= 0f)
            {
                // Recharge complete: restore all air jumps
                airJumpsLeft = maxAirJumps;
                // ground jump becomes available automatically in CanExecute
            }
        }
    }

    public override bool CanExecute(PlayerControllerRefactored player)
    {
        if (!IsEnabled) return false;

        // Check jump cooldown (prevents spamming)
        if (Time.time - lastJumpTime < jumpCooldown)
            return false;

        if (player.OnGround)
        {
            // On ground: can jump only if ground cooldown finished AND CanJump
            return groundCooldown <= 0f && player.CanJump;
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
            player.animator.SetBool("isJumping", true);
            player.animator.ResetTrigger("Jump");
            player.animator.SetTrigger("Jump");
            // Note: ground jump does NOT consume airJumpsLeft
        }
        else
        {
            // Air jump: consume one
            airJumpsLeft--;
            rb.AddForce(Vector3.up * airJumpForce, ForceMode.Impulse);
            PlayRandomClip(airJumpClips);
            player.animator.ResetTrigger("Jump");
            player.animator.SetTrigger("Jump");
        }
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        int idx = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[idx]);
    }
}