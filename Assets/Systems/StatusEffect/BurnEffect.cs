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
        // Grab all potential components we might need
        IBurnable burnable = target.GetComponent<IBurnable>();
        IDamageable damageable = target.GetComponent<IDamageable>();
        StatusEffectRunner runner = target.GetComponent<StatusEffectRunner>();
        EffectVFXHandler vfxHandler = target.GetComponent<EffectVFXHandler>();

        // 1. The New Puzzle Path (State Trigger)
        if (burnable != null)
        {
            Debug.Log("Burn APPLY fired on Puzzle Object: " + target.name);
            // The puzzle script (e.g., BurnableVine) will handle its own state/visuals here
            burnable.Ignite(duration); 
        }

        // 2. The Existing Enemy Path (Tick Damage & VFX via Coroutine)
        if (damageable != null && runner != null)
        {
            Debug.Log("Burn APPLY fired on Enemy: " + target.name);
            runner.StopEffect(vfxKey);
            runner.Run(vfxKey, BurnRoutine(target, damageable, vfxHandler));
        }
    }

    private IEnumerator BurnRoutine(GameObject target, IDamageable damageable, EffectVFXHandler vfxHandler)
    {
        // Attach VFX
        if (vfxHandler != null && fireVFXPrefab != null)
        {
            vfxHandler.AttachVFX(vfxKey, fireVFXPrefab);
        }

        float elapsed = 0f;

        // Tick Damage Loop
        while (elapsed < duration)
        {
            damageable.TakeDamage(tickDamage);
            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        // Remove VFX
        if (vfxHandler != null)
        {
            vfxHandler.RemoveVFX(vfxKey);
        }
    }
}