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
    [Tooltip("Player damage scripts use CompareTag(\"Enemy\"). Change tag when pacified so contact is safe.")]
    [SerializeField] private string pacifiedTag = "Untagged";
    [Tooltip("If set, assigns this layer (by name) on this object and all children — pair with Physics collision matrix so player doesn't treat hits as damage.")]
    [SerializeField] private string pacifiedLayerName = "";

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
    /// Call from <see cref="MushroomSequenceTracker.OnPuzzleSolved"/> or PuzzleValidator. Walks to the rest point,
    /// stays under the cap forever, removes Enemy damage (via tag/layer), and stops reacting to mushroom activations.
    /// </summary>
    public void PacifyAfterPuzzleSolve()
    {
        if (pacified) return;
        if (restPointUnderMushroom == null)
        {
            Debug.LogWarning($"[{nameof(CritterController)}] {name}: Assign Rest Point Under Mushroom (transform under the cap).");
            return;
        }

        Debug.Log($"[{nameof(CritterController)}] {name}: Pacify — move to rest point '{restPointUnderMushroom.name}' at {restPointUnderMushroom.position}, tag='{pacifiedTag}', layer='{pacifiedLayerName}'");

        pacified = true;
        if (mushroomChannel != null)
            mushroomChannel.OnRaised -= OnMushroomActivated;

        gameObject.tag = pacifiedTag;

        if (!string.IsNullOrEmpty(pacifiedLayerName))
        {
            int layer = LayerMask.NameToLayer(pacifiedLayerName);
            if (layer < 0)
                Debug.LogWarning($"[{nameof(CritterController)}] Layer '{pacifiedLayerName}' not found. Add it in Edit → Project Settings → Tags and Layers.");
            else
                SetLayerRecursively(transform, layer);
        }

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

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
            SetLayerRecursively(root.GetChild(i), layer);
    }
}
