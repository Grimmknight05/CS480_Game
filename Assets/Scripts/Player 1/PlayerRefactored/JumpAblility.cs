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
    }

    public override bool CanExecute(PlayerControllerRefactored player)
    {
        if (!IsEnabled) return false;
        if (player.OnGround && player.CanJump) return true;
        return airJumpsLeft > 0;
    }

    public override void Execute(PlayerControllerRefactored player)
    {
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
        }
        else
        {
            airJumpsLeft--;
            rb.AddForce(Vector3.up * airJumpForce, ForceMode.Impulse);
            PlayRandomClip(airJumpClips);
            player.animator.ResetTrigger("Jump");
            player.animator.SetTrigger("Jump");
        }
    }

    public void ResetOnGround()
    {
        airJumpsLeft = maxAirJumps;
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        int idx = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[idx]);
    }
}