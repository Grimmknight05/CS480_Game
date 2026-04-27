using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnEffect", menuName = "Status Effects/Burn")]
public class BurnEffect : StatusEffect
{
    [SerializeField] private int tickDamage = 5;
    [SerializeField] private float tickRate = 0.5f;

    [Header("VFX")]
    [SerializeField] private GameObject fireVFXPrefab;
    [SerializeField] private string vfxKey = "Burn";

    public override void Apply(GameObject target, Vector3 hitDirection)
    {
        var runner = target.GetComponent<StatusEffectRunner>();
        var health = target.GetComponent<EnemyControllerTest>();
        var vfxHandler = target.GetComponent<EffectVFXHandler>();

        if (runner != null && health != null)
        {
            runner.Run(BurnRoutine(target, health, vfxHandler));
        }
    }

    private IEnumerator BurnRoutine(GameObject target, EnemyControllerTest enemy, EffectVFXHandler vfxHandler)
    {
        GameObject vfx = null;

        if (vfxHandler != null && fireVFXPrefab != null)
        {
            vfx = vfxHandler.AttachVFX(vfxKey, fireVFXPrefab);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            enemy.TakeDamage(tickDamage);

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        // 🧹 Remove VFX when done
        if (vfxHandler != null)
        {
            vfxHandler.RemoveVFX(vfxKey);
        }
    }
}