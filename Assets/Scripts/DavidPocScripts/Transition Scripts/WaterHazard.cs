using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterHazard : MonoBehaviour
{
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private string playerTag = "Player";

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: WaterHazard collider was not a trigger; forcing IsTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (deathChannel == null)
        {
            Debug.LogError($"{gameObject.name}: WaterHazard has no deathChannel assigned.", this);
            return;
        }
        deathChannel.Raise();
    }
}
