using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStatusEffect : MonoBehaviour, IKnockbackable
{
    private NavMeshAgent agent;
    private Coroutine stunRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void ApplyKnockback(Vector3 direction, float force, float stunDuration)
    {
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(KnockbackRoutine(direction, force, stunDuration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float stunDuration)
    {
        agent.isStopped = true;

        agent.velocity = direction.normalized * force;

        yield return new WaitForSeconds(stunDuration);

        agent.velocity = Vector3.zero;
        agent.isStopped = false;

        stunRoutine = null;
    }
}