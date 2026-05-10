using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterHazard : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Damage applied per second when player is at least submergePlayerThreshold submerged.")]
    [SerializeField] private float damagePerSecond = 20f;
    [Tooltip("Submersion fraction (0-1) at or above which the player takes damage.")]
    [Range(0f, 1f)]
    [SerializeField] private float submergePlayerThreshold = 0.5f;
    [Tooltip("Apply damage in discrete ticks of this interval (seconds).")]
    [SerializeField] private float damageTickInterval = 0.5f;

    [Header("Pushable Reset")]
    [Tooltip("Submersion fraction (0-1) at or above which a Respawnable pushable resets to its spawn.")]
    [Range(0f, 1f)]
    [SerializeField] private float submergePushableThreshold = 0.9f;

    private Collider waterCollider;
    private readonly Dictionary<PlayerHealth, PlayerTracker> trackedPlayers = new();
    private readonly Dictionary<Respawnable, PushableTracker> trackedPushables = new();

    private class PlayerTracker
    {
        public GameObject root;
        public Collider bodyCollider;
        public int colliderCount;
        public float damageAccumulator;
    }

    private class PushableTracker
    {
        public GameObject root;
        public Collider bodyCollider;
        public int colliderCount;
    }

    void Awake()
    {
        waterCollider = GetComponent<Collider>();
        if (!waterCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: WaterHazard collider was not a trigger; forcing IsTrigger = true.", this);
            waterCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            if (!trackedPlayers.TryGetValue(health, out var tracker))
            {
                Collider body = FindBodyCollider(health.gameObject, preferCapsuleOrCC: true);
                if (body == null)
                {
                    Debug.LogWarning($"{gameObject.name}: WaterHazard could not find a body collider on player '{health.gameObject.name}'.", this);
                    return;
                }
                tracker = new PlayerTracker { root = health.gameObject, bodyCollider = body, colliderCount = 0, damageAccumulator = 0f };
                trackedPlayers.Add(health, tracker);
            }
            tracker.colliderCount++;
            return;
        }

        Respawnable respawnable = other.GetComponentInParent<Respawnable>();
        if (respawnable != null)
        {
            if (!trackedPushables.TryGetValue(respawnable, out var tr))
            {
                Collider body = FindBodyCollider(respawnable.gameObject, preferCapsuleOrCC: false);
                if (body == null) return;
                tr = new PushableTracker { root = respawnable.gameObject, bodyCollider = body, colliderCount = 0 };
                trackedPushables.Add(respawnable, tr);
            }
            tr.colliderCount++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null && trackedPlayers.TryGetValue(health, out var tracker))
        {
            tracker.colliderCount--;
            if (tracker.colliderCount <= 0) trackedPlayers.Remove(health);
            return;
        }

        Respawnable respawnable = other.GetComponentInParent<Respawnable>();
        if (respawnable != null && trackedPushables.TryGetValue(respawnable, out var tr))
        {
            tr.colliderCount--;
            if (tr.colliderCount <= 0) trackedPushables.Remove(respawnable);
        }
    }

    void Update()
    {
        TickPlayers();
        TickPushables();
    }

    private void TickPlayers()
    {
        if (trackedPlayers.Count == 0) return;

        float waterTopY = waterCollider.bounds.max.y;
        List<PlayerHealth> toRemove = null;

        foreach (var kv in trackedPlayers)
        {
            PlayerHealth health = kv.Key;
            PlayerTracker tracker = kv.Value;

            if (health == null || tracker.bodyCollider == null)
            {
                (toRemove ??= new List<PlayerHealth>()).Add(health);
                continue;
            }

            float fraction = SubmersionFraction(tracker.bodyCollider.bounds, waterTopY);
            if (fraction < submergePlayerThreshold)
            {
                tracker.damageAccumulator = 0f;
                continue;
            }

            tracker.damageAccumulator += Time.deltaTime;
            int damagePerTick = Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * damageTickInterval));
            while (tracker.damageAccumulator >= damageTickInterval)
            {
                tracker.damageAccumulator -= damageTickInterval;
                health.TakeDamage(damagePerTick);
                if (!health.IsAlive()) break;
            }
        }

        if (toRemove != null)
        {
            foreach (var h in toRemove) trackedPlayers.Remove(h);
        }
    }

    private void TickPushables()
    {
        if (trackedPushables.Count == 0) return;

        float waterTopY = waterCollider.bounds.max.y;
        List<Respawnable> toRemove = null;

        foreach (var kv in trackedPushables)
        {
            Respawnable r = kv.Key;
            PushableTracker tr = kv.Value;

            if (r == null || tr.bodyCollider == null)
            {
                (toRemove ??= new List<Respawnable>()).Add(r);
                continue;
            }

            float fraction = SubmersionFraction(tr.bodyCollider.bounds, waterTopY);
            if (fraction >= submergePushableThreshold)
            {
                r.ResetToSpawn();
                (toRemove ??= new List<Respawnable>()).Add(r);
            }
        }

        if (toRemove != null)
        {
            foreach (var r in toRemove) trackedPushables.Remove(r);
        }
    }

    // Resolves a single, stable "body" collider for submersion math.
    // For animated rigs, sampling combined-bounds across many child colliders
    // is unreliable because limb animation perturbs the AABB; using one
    // canonical capsule (CharacterController or main CapsuleCollider) gives
    // a steady reference volume.
    private static Collider FindBodyCollider(GameObject root, bool preferCapsuleOrCC)
    {
        if (preferCapsuleOrCC)
        {
            CharacterController cc = root.GetComponent<CharacterController>();
            if (cc != null) return cc;

            CapsuleCollider rootCap = root.GetComponent<CapsuleCollider>();
            if (rootCap != null && !rootCap.isTrigger) return rootCap;

            foreach (var c in root.GetComponentsInChildren<Collider>())
            {
                if (c == null || c.isTrigger) continue;
                if (c is CharacterController || c is CapsuleCollider) return c;
            }
        }

        foreach (var c in root.GetComponents<Collider>())
        {
            if (c != null && !c.isTrigger) return c;
        }
        foreach (var c in root.GetComponentsInChildren<Collider>())
        {
            if (c != null && !c.isTrigger) return c;
        }
        return null;
    }

    private static float SubmersionFraction(Bounds objBounds, float waterTopY)
    {
        if (objBounds.size.y <= 0f) return 0f;
        float submerged = Mathf.Clamp(waterTopY - objBounds.min.y, 0f, objBounds.size.y);
        return submerged / objBounds.size.y;
    }
}
