// CoreNode.cs
using UnityEngine;

public class CoreNode : MonoBehaviour
{
    [SerializeField] private CoreNodeIdentifierSO identifier;

    private DamageableObject damageable;

    private void Awake()
    {
        damageable = GetComponent<DamageableObject>();
        if (damageable == null)
            Debug.LogError("CoreNode requires a DamageableObject component!");
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