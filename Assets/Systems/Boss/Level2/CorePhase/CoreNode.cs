// CoreNode.cs
using UnityEngine;

public class CoreNode : MonoBehaviour
{
    [SerializeField] private CoreNodeIdentifierSO identifier;
    [SerializeField] private AudioSource idleAudioSource;

    private DamageableObject damageable;

    private void Awake()
    {
        damageable = GetComponent<DamageableObject>();
        if (damageable == null)
            Debug.LogError("CoreNode requires a DamageableObject component!");
        idleAudioSource.loop = true;
        idleAudioSource.Play();
    }

    private void OnEnable()
    {
        if (identifier != null)
            CoreNodeRegistry.Register(identifier, this);
    }

    private void OnDisable()
    {
        if (identifier != null)
            CoreNodeRegistry.Unregister(identifier);
    }

    public DamageableObject Damageable => damageable;
}