using UnityEngine;

public abstract class Tool : ScriptableObject
{
    [Header("Tool Properties")]
    [SerializeField] public string toolName = "Tool";
    [SerializeField] public string toolDescription = "A generic tool";
    [SerializeField] public float cooldown = 0.5f;
    [SerializeField] public AudioClip useSFX;
    [SerializeField] public Sprite toolIcon; // For UI display
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


}
