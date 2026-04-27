using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "KnockbackEffect", menuName = "Status Effects/Knockback")]
public class KnockbackEffect : StatusEffect
{
    [SerializeField] private float force = 10f;
    [SerializeField] private float stunDuration = 0.3f;

    public override void Apply(GameObject target, Vector3 hitDirection)
    {
        EnemyStatusEffect status = target.GetComponent<EnemyStatusEffect>();
        if (status != null)
        {
            status.ApplyKnockback(hitDirection, force, stunDuration);
        }
    }
}