using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyBlastReceiver : MonoBehaviour, IBlastReceiver, IKnockbackable
{
    [SerializeField] private float pushForce = 15f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnBlast(Vector3 origin, float normalizedFalloff)
    {
        Debug.Log($"<color=magenta>[Receiver]</color> {gameObject.name} RECEIVED SIGNAL! Calculating force...");

        Vector3 pushDirection = (transform.position - origin).normalized;
        float finalForce = pushForce * normalizedFalloff;

        Debug.Log($"<color=magenta>[Receiver]</color> Pushing {gameObject.name} with force {finalForce}. Is Kinematic? {rb.isKinematic}. Mass: {rb.mass}");

        // VelocityChange completely ignores the mass of the Rigidbody
        rb.AddForce(pushDirection * finalForce, ForceMode.VelocityChange);
    }

    // Targeted push (PushTool) — direction supplied by the caller, not derived radially.
    // stunDuration is unused: a passive rigidbody has no agent/state to halt.
    public void ApplyKnockback(Vector3 direction, float force, float stunDuration)
    {
        Debug.Log($"<color=magenta>[Receiver]</color> {gameObject.name} KNOCKED BACK! Direction: {direction}, Force: {force}");
        rb.AddForce(direction.normalized * force, ForceMode.VelocityChange);
    }
}