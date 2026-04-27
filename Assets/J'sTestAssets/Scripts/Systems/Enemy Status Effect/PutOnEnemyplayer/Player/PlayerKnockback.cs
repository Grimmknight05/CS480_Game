using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerKnockback : MonoBehaviour, IKnockbackable
{
    private CharacterController controller;
    private Coroutine knockRoutine;

    private Vector3 knockVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void ApplyKnockback(Vector3 direction, float force, float stunDuration)
    {
        if (knockRoutine != null)
            StopCoroutine(knockRoutine);

        knockRoutine = StartCoroutine(KnockbackRoutine(direction, force, stunDuration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float stunDuration)
    {
        float elapsed = 0f;
        Vector3 dir = direction.normalized;

        while (elapsed < stunDuration)
        {
            knockVelocity = dir * force;
            controller.Move(knockVelocity * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        knockVelocity = Vector3.zero;
        knockRoutine = null;
    }
}