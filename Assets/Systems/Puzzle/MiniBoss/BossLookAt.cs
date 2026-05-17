using UnityEngine;

public class BossLookAt : MonoBehaviour, ILock
{
    [Header("Target")]
    [SerializeField] private Transform objectToRotate; // optional, uses this transform if null

    [Header("Player Detection")]
    [Tooltip("If true, automatically find player by tag. If false, you must assign playerTransform manually.")]
    [SerializeField] private bool autoFindPlayer = true;

    [Tooltip("The player's transform (optional if autoFindPlayer is true).")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Seconds between retry attempts when the player cannot be found.")]
    [SerializeField] private float playerSearchInterval = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool resetOnDeactivate = true;

    [Header("Optional Visual")]
    [SerializeField] private ParticleSystem activationVFX;
    [SerializeField] private ParticleSystem deactivationVFX;
    [Header("Rotation Constraints")]
    [SerializeField] private bool lockToYAxis = true;   // if true, only rotate horizontally
    private bool _lockState = false;
    public bool LockState { get => _lockState; set => _lockState = value; }
    public void Lock(bool lockstate) => LockState = lockstate;

    private bool isActive = false;
    private Quaternion originalRotation;
    private bool playerSearchFailed = false; // avoid spamming Find every frame
    private float _nextPlayerSearchTime;

    private void Awake()
    {
        if (objectToRotate == null)
            objectToRotate = transform;

        originalRotation = objectToRotate.localRotation;

        if (autoFindPlayer)
        {
            // Try to find player immediately
            TryFindPlayer();
        }
        else if (playerTransform == null)
        {
            Debug.LogWarning("BossLookAt: autoFindPlayer is false but playerTransform is not assigned.");
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerSearchFailed = false;
            Debug.Log("BossLookAt: Player found automatically.");
        }
        else
        {
            playerSearchFailed = true;
            _nextPlayerSearchTime = Time.time + playerSearchInterval;
            Debug.LogWarning($"BossLookAt: No GameObject with tag 'Player' found. Will retry every {playerSearchInterval}s until found.");
        }
    }

    public void Activate()
    {
        if (LockState) return;
        isActive = true;
        activationVFX?.Play();
    }

    public void Deactivate()
    {
        if (LockState) return;
        isActive = false;
        if (resetOnDeactivate)
            objectToRotate.localRotation = originalRotation;
        deactivationVFX?.Play();
    }

    private void Update()
    {
        if (!isActive || LockState) return;

        // Ensure we have a player reference
        if (playerTransform == null)
        {
            if (autoFindPlayer)
            {
                if (playerSearchFailed && Time.time < _nextPlayerSearchTime) return;
                TryFindPlayer();
                if (playerTransform == null) return;
            }
            else
            {
                return;
            }
        }

        // Calculate direction to player but ignore the Y (vertical) component
        Vector3 direction = playerTransform.position - objectToRotate.position;
        if (lockToYAxis) direction.y = 0f;                     // <-- lock to Y axis (only horizontal rotation)

        if (direction.magnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (rotationSpeed <= 0f)
        {
            // Instant lock to Y axis
            objectToRotate.rotation = targetRotation;
        }
        else
        {
            // Smooth rotation around Y only
            objectToRotate.rotation = Quaternion.RotateTowards(
                objectToRotate.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}