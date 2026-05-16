using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Critter actor. Subscribes to MushroomEventChannelSO in OnEnable / OnDisable
// (Observer pattern, with the OnEnable+OnDisable hygiene EventChannelSO requires
// because the channel clears subscribers when its asset is disabled).
// Per-frame behavior is delegated to a CritterState (State pattern).

[RequireComponent(typeof(Rigidbody))]
public class CritterController : MonoBehaviour
{
    [Header("Channel")]
    [SerializeField] private MushroomEventChannelSO mushroomChannel;

    [Header("Hop tuning")]
    [SerializeField] private float hopIntervalMin = 0.6f;
    [SerializeField] private float hopIntervalMax = 1.4f;
    [SerializeField] private float hopVertical = 4f;
    [SerializeField] private float hopHorizontal = 1.5f;

    [Header("Move tuning")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float arriveRadius = 1.0f;
    [SerializeField] private float secondsToReachCap = 3f;

    [Header("After mushroom puzzle solved")]
    [Tooltip("Place an empty transform slightly under the mushroom cap — critter walks here and stays.")]
    [SerializeField] private Transform restPointUnderMushroom;
    [Tooltip("Player damage scripts often use CompareTag(\"Enemy\"). Cleared when pacified.")]
    [SerializeField] private string pacifiedTag = "Untagged";

    private Rigidbody body;
    private CritterState currentState;
    private bool pacified;

    public Rigidbody Body => body;
    public float HopIntervalMin => hopIntervalMin;
    public float HopIntervalMax => hopIntervalMax;
    public float HopVertical => hopVertical;
    public float HopHorizontal => hopHorizontal;
    public float WalkSpeed => walkSpeed;
    public float ArriveRadius => arriveRadius;
    public CritterState CurrentState => currentState;
    public bool IsPacified => pacified;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (!pacified)
            SetState(new JumpingState());
    }

    private void OnEnable()
    {
        if (pacified) return;
        if (mushroomChannel != null) mushroomChannel.OnRaised += OnMushroomActivated;
    }

    private void OnDisable()
    {
        if (mushroomChannel != null) mushroomChannel.OnRaised -= OnMushroomActivated;
    }

    private void Update()
    {
        currentState?.Tick(this);
    }

    public void SetState(CritterState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    /// <summary>
    /// Walks to the rest point and stays calm. Removes Enemy-tag contact damage routing and zeros
    /// <see cref="EnemyControllerTest"/> combat if present (runtime attack clone — shared AttackData asset untouched).
    /// </summary>
    public void PacifyAfterPuzzleSolve()
    {
        if (pacified) return;
        if (restPointUnderMushroom == null)
        {
            Debug.LogWarning($"[{nameof(CritterController)}] {name}: Assign Rest Point Under Mushroom (transform under the cap).");
            return;
        }

        Debug.Log($"[{nameof(CritterController)}] {name}: Pacify — rest '{restPointUnderMushroom.name}' at {restPointUnderMushroom.position}, tag='{pacifiedTag}', combat cleared via EnemyController if any.");

        pacified = true;
        if (mushroomChannel != null)
            mushroomChannel.OnRaised -= OnMushroomActivated;

        gameObject.tag = pacifiedTag;

        var enemy = GetComponent<EnemyControllerTest>();
        if (enemy == null) enemy = GetComponentInChildren<EnemyControllerTest>();
        enemy?.SetPacifiedCombat();

        if (body != null)
            body.isKinematic = true;

        SetState(new MovingState(
            restPointUnderMushroom.position,
            arriveRadius,
            secondsToReachCap,
            calmDuration: 1f,
            calmPermanent: true));
    }

    private void OnMushroomActivated(MushroomActivationData data)
    {
        if (pacified) return;

        var flat = transform.position - data.Position;
        flat.y = 0f;
        if (flat.sqrMagnitude > data.CalmRadius * data.CalmRadius) return;
        if (currentState is CalmUnderCapState) return;

        SetState(new MovingState(data.Position, arriveRadius, secondsToReachCap, data.Duration));
    }
}
