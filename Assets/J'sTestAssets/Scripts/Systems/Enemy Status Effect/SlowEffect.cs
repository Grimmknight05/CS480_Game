using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "SlowEffect", menuName = "Status Effects/Slow")]
public class SlowEffect : StatusEffect
{
    [Range(0f, 1f)]
    [SerializeField] private float slowMultiplier = 0.5f;

    public override void Apply(GameObject target, Vector3 hitDirection)
    {
        var runner = target.GetComponent<StatusEffectRunner>();
        var agent = target.GetComponent<NavMeshAgent>();

        if (runner != null && agent != null)
        {
            runner.Run(SlowRoutine(agent));
        }
    }

    private IEnumerator SlowRoutine(NavMeshAgent agent)
    {
        float originalSpeed = agent.speed;

        agent.speed *= slowMultiplier;

        yield return new WaitForSeconds(duration);

        if (agent != null)
            agent.speed = originalSpeed;
    }
}