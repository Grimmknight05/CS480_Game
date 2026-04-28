// =====================================================================
// Author: Katie Trinh
// File:   RobotAnimationDriver.cs
// Folder: Assets/Scripts/KatiePOCScripts/
//
// Drives the Quaternius "Animated Robot" prefab.
//   * Configures the Rigidbody on Awake so the player CAN'T walk through
//     the robot (kinematic body + non-trigger collider blocks movement).
//   * Plays a named Animator state on Start (default: "Robot_Idle") so
//     the robot is idling in place.

// =====================================================================

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RobotAnimationDriver : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Animator on this GameObject or a child. Auto-found if left null.")]
    [SerializeField] private Animator animator;

    [Tooltip("Name of the Animator state to play on Start. The Quaternius FBX " +
             "clip is named 'RobotArmature|Robot_Idle' by default. If you rename " +
             "the Animator state, update this field to match.")]
    [SerializeField] private string idleStateName = "RobotArmature|Robot_Idle";

    [Header("Rigidbody setup")]
    [Tooltip("If true, the Rigidbody is configured as kinematic so the robot " +
             "never falls or is pushed but still blocks the player.")]
    [SerializeField] private bool makeKinematic = true;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        PlayIdleState();
    }

    private void ConfigureRigidbody()
    {
        rb.isKinematic = makeKinematic;
        rb.useGravity = !makeKinematic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = makeKinematic
            ? CollisionDetectionMode.ContinuousSpeculative
            : CollisionDetectionMode.Continuous;
        // Even when not kinematic, freeze rotation so the robot stays upright.
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    private void PlayIdleState()
    {
        if (animator == null || string.IsNullOrWhiteSpace(idleStateName))
        {
            return;
        }

        int hash = Animator.StringToHash(idleStateName);
        if (animator.HasState(0, hash))
        {
            animator.Play(hash, 0, 0f);
        }
    }
}
