using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/Knockback")]
public class KnockbackEffect : StatusEffect
{
    [SerializeField] private float force = 5f;
    [SerializeField] private float stunDuration = 0.2f;

    public override void Apply(GameObject target, Vector3 hitDirection)
    {
        var knockback = target.GetComponent<IKnockbackable>();

        if (knockback != null)
        {
            knockback.ApplyKnockback(hitDirection, force, stunDuration);
        }
    }
}