using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnEffect", menuName = "Status Effects/Burn")]
public class BurnEffect : StatusEffect
{
    [Header("Damage")]
    [SerializeField] private int tickDamage = 5;
    [SerializeField] private float tickRate = 0.5f;

    [Header("VFX")]
    [SerializeField] private GameObject fireVFXPrefab;
    [SerializeField] private string vfxKey = "Burn";

    public override void Apply(GameObject target, Vector3 hitDirection)
    {
        var runner = target.GetComponent<StatusEffectRunner>();
        var damageable = target.GetComponent<IDamageable>();
        var vfxHandler = target.GetComponent<EffectVFXHandler>();

        if (runner == null || damageable == null)
            return;
        Debug.Log("Burn APPLY fired on " + target.name);
        runner.StopEffect(vfxKey);
        runner.Run(vfxKey, BurnRoutine(target, damageable, vfxHandler));
    }

    private IEnumerator BurnRoutine(GameObject target, IDamageable damageable, EffectVFXHandler vfxHandler)
    {
        if (vfxHandler != null && fireVFXPrefab != null)
        {
            vfxHandler.AttachVFX(vfxKey, fireVFXPrefab);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            damageable.TakeDamage(tickDamage);

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        if (vfxHandler != null)
        {
            vfxHandler.RemoveVFX(vfxKey);
        }
    }
}