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

    private Rigidbody body;
    private CritterState currentState;

    public Rigidbody Body => body;
    public float HopIntervalMin => hopIntervalMin;
    public float HopIntervalMax => hopIntervalMax;
    public float HopVertical => hopVertical;
    public float HopHorizontal => hopHorizontal;
    public float WalkSpeed => walkSpeed;
    public float ArriveRadius => arriveRadius;
    public CritterState CurrentState => currentState;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetState(new JumpingState());
    }

    private void OnEnable()
    {
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

    private void OnMushroomActivated(MushroomActivationData data)
    {
        var flat = transform.position - data.Position;
        flat.y = 0f;
        if (flat.sqrMagnitude > data.CalmRadius * data.CalmRadius) return;
        if (currentState is CalmUnderCapState) return;

        SetState(new MovingState(data.Position, arriveRadius, secondsToReachCap, data.Duration));
    }
}
