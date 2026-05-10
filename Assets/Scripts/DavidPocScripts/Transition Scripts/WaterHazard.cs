using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterHazard : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

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
    private readonly Dictionary<Respawnable, Collider> trackedPushables = new();

    private class PlayerTracker
    {
        public Collider collider;
        public float damageAccumulator;
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
        if (other.CompareTag(playerTag))
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health != null && !trackedPlayers.ContainsKey(health))
            {
                trackedPlayers.Add(health, new PlayerTracker { collider = other, damageAccumulator = 0f });
            }
            return;
        }

        Respawnable respawnable = other.GetComponentInParent<Respawnable>();
        if (respawnable != null && !trackedPushables.ContainsKey(respawnable))
        {
            trackedPushables.Add(respawnable, other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health != null) trackedPlayers.Remove(health);
            return;
        }

        Respawnable respawnable = other.GetComponentInParent<Respawnable>();
        if (respawnable != null) trackedPushables.Remove(respawnable);
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

            if (health == null || tracker.collider == null)
            {
                (toRemove ??= new List<PlayerHealth>()).Add(health);
                continue;
            }

            float fraction = SubmersionFraction(tracker.collider.bounds, waterTopY);
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
            Collider c = kv.Value;

            if (r == null || c == null)
            {
                (toRemove ??= new List<Respawnable>()).Add(r);
                continue;
            }

            float fraction = SubmersionFraction(c.bounds, waterTopY);
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

    private static float SubmersionFraction(Bounds objBounds, float waterTopY)
    {
        if (objBounds.size.y <= 0f) return 0f;
        float submerged = Mathf.Clamp(waterTopY - objBounds.min.y, 0f, objBounds.size.y);
        return submerged / objBounds.size.y;
    }
}
