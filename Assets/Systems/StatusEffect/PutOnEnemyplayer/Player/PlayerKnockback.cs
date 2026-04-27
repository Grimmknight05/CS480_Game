using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour, IKnockbackable
{
    private Rigidbody rb;
    private Coroutine routine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyKnockback(Vector3 direction, float force, float stunDuration)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(KnockbackRoutine(direction, force, stunDuration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float stunDuration)
    {
        rb.linearVelocity = direction.normalized * force;

        yield return new WaitForSeconds(stunDuration);

        rb.linearVelocity = Vector3.zero;
        routine = null;
    }
}