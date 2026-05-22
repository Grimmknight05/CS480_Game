using UnityEngine;
public abstract class Weapon : TargetingTool
{
    [Header("Weapon Properties")]
    [SerializeField] public int damagePerHit = 20;

    /// <summary>
    /// Fire method for weapons. Convenience wrapper around Use().
    /// </summary>
    public virtual void Fire(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {
        Use(firePoint, audioSource, layerMask);
    }
}