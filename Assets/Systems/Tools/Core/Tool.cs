using UnityEngine;

public abstract class Tool : ScriptableObject
{
    [Header("Tool Properties")]
    [SerializeField] public string toolName = "Tool";
    [SerializeField] public string toolDescription = "A generic tool";
    [SerializeField] public float cooldown = 0.5f;
    
    [SerializeField] public Sprite toolIcon; // For UI display
    [Header("Audio")]
    [SerializeField] public AudioClip useSFX;
    [SerializeField] private AudioClip changeToolSFX;
    [Header("Visuals")]
    public GameObject visualPrefab;   // model to attach to player's weapon mount
    [Header("Temp Weapon")]
    public bool isTempWeapon = false; // if true, equipping hides all other tools
    [Header("Targeting")]
    [SerializeField] protected LayerMask targetLayer;
    [Header("Status Effects")]
    [SerializeField] protected StatusEffect[] effects;

    public LayerMask GetTargetLayer()
    {
        return targetLayer;
    }

    public abstract void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask);

    protected void PlayUseSound(AudioSource audioSource)
    {
        if (useSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSFX);
        }
    }
    public void PlayChangeToolSound(AudioSource audioSource)
    {
        if (changeToolSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(changeToolSFX);
        }
    }
    /// <summary>
    /// Applies all status effects assigned to this tool to the target.
    /// </summary>
    protected void ApplyEffects(GameObject target, Vector3 hitDirection)
    {
        if (effects == null || effects.Length == 0) return;

        foreach (StatusEffect effect in effects)
        {
            effect.Apply(target, hitDirection);
        }
    }
    


}
