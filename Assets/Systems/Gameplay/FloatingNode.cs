using UnityEngine;

public class FloatingNode : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("Zero-G Room Drift")]
    [SerializeField] private bool driftWithinBounds;
    [SerializeField] private bool randomizeDrift = true;
    [SerializeField] private Vector3 driftVelocity = new Vector3(0.35f, 0.15f, 0.25f);
    [SerializeField] private Vector2 randomDriftSpeedRange = new Vector2(0.25f, 0.65f);
    [SerializeField] private BoxCollider boundsBox;
    [SerializeField] private Transform boundsCenter;
    [SerializeField] private Vector3 boundsSize = new Vector3(6f, 3f, 6f);

    [Header("Zero-G Collision")]
    [SerializeField] private bool usePhysicsCollisions = true;
    [SerializeField] private bool autoCreateCollisionBody = true;
    [SerializeField] private bool mergeChildRigidbodies = true;
    [SerializeField] private float collisionPadding = 0.1f;
    [SerializeField] private float collisionBounciness = 0.6f;
    [SerializeField] private float minimumDriftSpeed = 0.15f;
    [SerializeField] private float maximumDriftSpeed = 1.25f;

    [Header("Shield Visual")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Material damagedMaterial;
    [SerializeField] private float damagedFlashDuration = 0.2f;
    
    private Vector3 startPosition;
    private Vector3 driftPosition;
    private Vector3 currentDriftVelocity;
    private float currentShieldHealth = 1f;
    private Material originalMaterial;
    private Renderer shieldRenderer;
    private Rigidbody driftRigidbody;
    private PhysicsMaterial driftPhysicMaterial;
    private Collider[] solidColliders;
    private Renderer[] cachedRenderers;

    private bool UsesPhysicsDrift => driftWithinBounds && usePhysicsCollisions && driftRigidbody != null;

    private void Awake()
    {
        SanitizeSettings();
        CacheRenderers();
        ConfigureBoundsBox();
    }

    private void OnValidate()
    {
        SanitizeSettings();
        ConfigureBoundsBox();
    }
    
    void Start()
    {
        SanitizeSettings();
        CacheRenderers();
        ConfigureBoundsBox();
        startPosition = transform.position;
        driftPosition = transform.position;
        currentDriftVelocity = randomizeDrift
            ? RandomDirection() * Random.Range(randomDriftSpeedRange.x, randomDriftSpeedRange.y)
            : driftVelocity;
        ConfigureCollisionDrift();
        
        if (shieldVisual != null)
        {
            shieldRenderer = shieldVisual.GetComponent<Renderer>();
            if (shieldRenderer != null)
                originalMaterial = shieldRenderer.material;
        }
    }

    private void OnDestroy()
    {
        if (driftPhysicMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(driftPhysicMaterial);
    }
    
    void Update()
    {
        if (UsesPhysicsDrift)
            return;

        if (driftWithinBounds)
            UpdateDriftingFloat();
        else
            UpdateStationaryFloat();
        
        // Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (UsesPhysicsDrift)
            UpdatePhysicsDrift();
    }

    private void UpdateStationaryFloat()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void UpdateDriftingFloat()
    {
        driftPosition += currentDriftVelocity * Time.deltaTime;
        BounceInsideBounds();

        float bob = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = driftPosition + Vector3.up * bob;
    }

    private void BounceInsideBounds()
    {
        BounceInsideBounds(ref driftPosition, ref currentDriftVelocity);
    }

    private void BounceInsideBounds(ref Vector3 position, ref Vector3 velocity)
    {
        Vector3 center = CurrentBoundsCenter;
        Vector3 halfSize = CurrentBoundsSize * 0.5f;
        Vector3 roomMin = center - halfSize;
        Vector3 roomMax = center + halfSize;
        GetCurrentObjectOffsets(position, out Vector3 minOffset, out Vector3 maxOffset);
        Vector3 min = roomMin - minOffset;
        Vector3 max = roomMax - maxOffset;

        BounceAxis(ref position.x, ref velocity.x, min.x, max.x);
        BounceAxis(ref position.y, ref velocity.y, min.y, max.y);
        BounceAxis(ref position.z, ref velocity.z, min.z, max.z);
    }

    private void BounceAxis(ref float position, ref float velocity, float min, float max)
    {
        if (min > max)
        {
            position = (min + max) * 0.5f;
            velocity = -velocity;
            return;
        }

        if (position < min)
        {
            position = min;
            velocity = Mathf.Abs(velocity);
        }
        else if (position > max)
        {
            position = max;
            velocity = -Mathf.Abs(velocity);
        }
    }

    private void GetCurrentObjectOffsets(Vector3 pivotPosition, out Vector3 minOffset, out Vector3 maxOffset)
    {
        if (TryGetCombinedColliderBounds(out Bounds solidBounds) || TryGetCombinedRendererBounds(out solidBounds))
        {
            minOffset = solidBounds.min - pivotPosition;
            maxOffset = solidBounds.max - pivotPosition;
            return;
        }

        minOffset = Vector3.zero;
        maxOffset = Vector3.zero;
    }

    private bool TryGetCombinedColliderBounds(out Bounds combinedBounds)
    {
        combinedBounds = new Bounds(transform.position, Vector3.zero);
        if (solidColliders == null || solidColliders.Length == 0)
            return false;

        bool hasBounds = false;
        foreach (Collider collider in solidColliders)
        {
            if (collider == null || !collider.enabled)
                continue;

            if (!hasBounds)
            {
                combinedBounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    private bool TryGetCombinedRendererBounds(out Bounds combinedBounds)
    {
        combinedBounds = new Bounds(transform.position, Vector3.zero);
        if (cachedRenderers == null || cachedRenderers.Length == 0)
            return false;

        bool hasBounds = false;
        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null || !renderer.enabled)
                continue;

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private Vector3 RandomDirection()
    {
        Vector3 direction = Random.onUnitSphere;
        return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.up;
    }

    private Vector3 CurrentBoundsCenter
    {
        get
        {
            if (boundsBox != null)
                return boundsBox.bounds.center;

            if (boundsCenter != null)
                return boundsCenter.position;

            return startPosition;
        }
    }

    private Vector3 CurrentBoundsSize => boundsBox != null ? boundsBox.bounds.size : boundsSize;

    private void SanitizeSettings()
    {
        randomDriftSpeedRange.x = Mathf.Max(0f, randomDriftSpeedRange.x);
        randomDriftSpeedRange.y = Mathf.Max(randomDriftSpeedRange.x, randomDriftSpeedRange.y);
        boundsSize = new Vector3(
            Mathf.Max(boundsSize.x, 0.05f),
            Mathf.Max(boundsSize.y, 0.05f),
            Mathf.Max(boundsSize.z, 0.05f));
        collisionPadding = Mathf.Max(0f, collisionPadding);
        collisionBounciness = Mathf.Clamp01(collisionBounciness);
        minimumDriftSpeed = Mathf.Max(0f, minimumDriftSpeed);

        if (maximumDriftSpeed > 0f)
            maximumDriftSpeed = Mathf.Max(maximumDriftSpeed, minimumDriftSpeed);
    }

    private void CacheRenderers()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
    }

    private void ConfigureBoundsBox()
    {
        if (boundsBox != null)
            boundsBox.isTrigger = true;
    }

    private void ConfigureCollisionDrift()
    {
        if (!driftWithinBounds || !usePhysicsCollisions)
            return;

        driftRigidbody = GetComponent<Rigidbody>();
        if (driftRigidbody == null && autoCreateCollisionBody)
            driftRigidbody = gameObject.AddComponent<Rigidbody>();

        if (driftRigidbody != null && mergeChildRigidbodies)
            MergeChildRigidbodies();

        Collider solidCollider = FindSolidCollider();
        if (solidCollider == null && autoCreateCollisionBody)
            solidCollider = CreateRootBoxCollider();

        CacheSolidColliders();
        ApplyBounceMaterialToSolidColliders();

        if (driftRigidbody == null)
            return;

        driftRigidbody.useGravity = false;
        driftRigidbody.isKinematic = false;
        driftRigidbody.linearDamping = 0f;
        driftRigidbody.angularDamping = 0.05f;
        driftRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        driftRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        driftRigidbody.sleepThreshold = 0f;
        driftRigidbody.linearVelocity = ClampDriftSpeed(currentDriftVelocity);
        driftRigidbody.angularVelocity = Vector3.up * rotateSpeed * Mathf.Deg2Rad;
    }

    private void MergeChildRigidbodies()
    {
        Rigidbody[] childBodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in childBodies)
        {
            if (body == null || body == driftRigidbody)
                continue;

            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            if (Application.isPlaying)
                Destroy(body);
        }
    }

    private Collider FindSolidCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if (IsSolidDriftCollider(collider))
                return collider;
        }

        return null;
    }

    private void CacheSolidColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        int solidCount = 0;
        foreach (Collider collider in colliders)
        {
            if (IsSolidDriftCollider(collider))
                solidCount++;
        }

        if (solidCount == 0)
        {
            solidColliders = null;
            return;
        }

        solidColliders = new Collider[solidCount];
        int index = 0;
        foreach (Collider collider in colliders)
        {
            if (IsSolidDriftCollider(collider))
                solidColliders[index++] = collider;
        }
    }

    private bool IsSolidDriftCollider(Collider collider)
    {
        return collider != null && collider != boundsBox && !collider.isTrigger;
    }

    private BoxCollider CreateRootBoxCollider()
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        Bounds localBounds = CalculateLocalRendererBounds();
        collider.center = localBounds.center;
        collider.size = localBounds.size;
        collider.isTrigger = false;
        return collider;
    }

    private Bounds CalculateLocalRendererBounds()
    {
        Renderer[] renderers = cachedRenderers ?? GetComponentsInChildren<Renderer>();
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            Bounds rendererBounds = renderer.bounds;
            Vector3 min = rendererBounds.min;
            Vector3 max = rendererBounds.max;

            for (int x = 0; x <= 1; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    for (int z = 0; z <= 1; z++)
                    {
                        Vector3 worldPoint = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

                        if (!hasBounds)
                        {
                            localBounds = new Bounds(localPoint, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(localPoint);
                        }
                    }
                }
            }
        }

        if (!hasBounds)
            localBounds = new Bounds(Vector3.zero, Vector3.one);

        localBounds.Expand(collisionPadding * 2f);
        localBounds.size = new Vector3(
            Mathf.Max(localBounds.size.x, 0.05f),
            Mathf.Max(localBounds.size.y, 0.05f),
            Mathf.Max(localBounds.size.z, 0.05f));
        return localBounds;
    }

    private void ApplyBounceMaterialToSolidColliders()
    {
        if (solidColliders == null || solidColliders.Length == 0)
            return;

        if (driftPhysicMaterial == null)
        {
            driftPhysicMaterial = new PhysicsMaterial($"{name} Zero-G Bounce")
            {
                bounciness = collisionBounciness,
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
        }

        foreach (Collider collider in solidColliders)
        {
            if (collider == null)
                continue;

            collider.material = driftPhysicMaterial;
        }
    }

    private void UpdatePhysicsDrift()
    {
        Vector3 position = driftRigidbody.position;
        Vector3 velocity = driftRigidbody.linearVelocity;

        if (velocity.sqrMagnitude < minimumDriftSpeed * minimumDriftSpeed)
            velocity = currentDriftVelocity.sqrMagnitude > 0.001f
                ? currentDriftVelocity.normalized * minimumDriftSpeed
                : RandomDirection() * minimumDriftSpeed;

        BounceInsideBounds(ref position, ref velocity);

        if ((position - driftRigidbody.position).sqrMagnitude > 0.0001f)
            driftRigidbody.MovePosition(position);

        velocity = ClampDriftSpeed(velocity);
        driftRigidbody.linearVelocity = velocity;
        currentDriftVelocity = velocity;
    }

    private Vector3 ClampDriftSpeed(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.001f)
            velocity = RandomDirection() * minimumDriftSpeed;

        if (maximumDriftSpeed > 0f && velocity.sqrMagnitude > maximumDriftSpeed * maximumDriftSpeed)
            velocity = velocity.normalized * maximumDriftSpeed;

        return velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!UsesPhysicsDrift || collision.contactCount == 0)
            return;

        Vector3 normal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
            normal += collision.GetContact(i).normal;

        if (normal.sqrMagnitude < 0.001f)
            return;

        normal.Normalize();
        Vector3 velocity = driftRigidbody.linearVelocity;
        if (Vector3.Dot(velocity, normal) < 0f)
            velocity = Vector3.Reflect(velocity, normal) * Mathf.Max(collisionBounciness, 0.1f);

        driftRigidbody.linearVelocity = ClampDriftSpeed(velocity);
        currentDriftVelocity = driftRigidbody.linearVelocity;
    }
    
    public void UpdateShieldHealth(float healthPercent)
    {
        currentShieldHealth = healthPercent;
        
        if (shieldRenderer != null && damagedMaterial != null)
        {
            StartCoroutine(FlashDamaged());
        }
    }
    
    private System.Collections.IEnumerator FlashDamaged()
    {
        shieldRenderer.material = damagedMaterial;
        yield return new WaitForSeconds(damagedFlashDuration);
        shieldRenderer.material = originalMaterial;
    }

    private void OnDrawGizmosSelected()
    {
        if (!driftWithinBounds)
            return;

        Vector3 center = boundsBox != null
            ? boundsBox.bounds.center
            : boundsCenter != null
                ? boundsCenter.position
                : transform.position;

        Vector3 size = boundsBox != null ? boundsBox.bounds.size : boundsSize;
        Gizmos.color = new Color(0.25f, 0.9f, 1f, 0.45f);
        Gizmos.DrawWireCube(center, size);
    }
}
