using UnityEngine;

/// <summary>
/// Drop on any GameObject that has colliders — logs touches so you can verify layer/tag/trigger-vs-collision.
/// Useful when <see cref="InteractableTrigger"/> is not wired yet or <c>Is Trigger</c> is wrong.
/// </summary>
public class PhysicsInteractionTouchDebug : MonoBehaviour
{
    [Header("Logs")]
    [SerializeField] private bool logTriggerEnter = true;
    [SerializeField] private bool logTriggerExit = true;
    [SerializeField] private bool logCollisionEnter = true;
    [SerializeField] private bool logTriggerStay;

    [Tooltip("Minimum seconds between OnTriggerStay logs when Log Trigger Stay is on.")]
    [SerializeField] private float triggerStayLogIntervalSeconds = 0.5f;

    private float _lastStayLogTime = float.NegativeInfinity;

    private void OnTriggerEnter(Collider other)
    {
        if (!logTriggerEnter) return;
        Debug.Log($"[TouchDebug:{name}] OnTriggerEnter ← '{other.name}' tag={other.tag} layer={LayerMask.LayerToName(other.gameObject.layer)} rigidbody={(other.attachedRigidbody != null)}", this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!logTriggerExit) return;
        Debug.Log($"[TouchDebug:{name}] OnTriggerExit ← '{other.name}' tag={other.tag}", this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!logTriggerStay) return;
        if (Time.time - _lastStayLogTime < triggerStayLogIntervalSeconds) return;
        _lastStayLogTime = Time.time;
        Debug.Log($"[TouchDebug:{name}] OnTriggerStay ← '{other.name}' (inside volume)", this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!logCollisionEnter) return;
        var cc = collision.collider;
        Debug.Log($"[TouchDebug:{name}] OnCollisionEnter ← '{cc.name}' tag={cc.tag} layer={LayerMask.LayerToName(cc.gameObject.layer)} (IsTrigger={cc.isTrigger}). For InteractableTrigger you usually want collider Is Trigger.", this);
    }
}
