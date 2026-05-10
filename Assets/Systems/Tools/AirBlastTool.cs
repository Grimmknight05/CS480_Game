using UnityEngine;

[CreateAssetMenu(fileName = "AirBlastTool", menuName = "Tools/Air Blast")]
public class AirBlastTool : Tool
{
    [Header("Air Blast Settings")]
    [SerializeField] private float effectRadius = 15f;
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {
        PlayUseSound(audioSource);
        
        Debug.Log($"<color=cyan>[Air Blast]</color> Fired from {usePoint.position}. Searching layer mask: {layerMask.value}");

        Collider[] hits = Physics.OverlapSphere(usePoint.position, effectRadius, layerMask);
        Debug.Log($"<color=cyan>[Air Blast]</color> Found {hits.Length} colliders in radius.");

        foreach (Collider hitCollider in hits)
        {
            float distance = Vector3.Distance(usePoint.position, hitCollider.transform.position);
            float normalizedDistance = Mathf.Clamp01(distance / effectRadius);
            float falloff = falloffCurve.Evaluate(normalizedDistance);

            Debug.Log($"<color=yellow>[Air Blast]</color> Checking object: {hitCollider.gameObject.name} | Distance: {distance}");

            IBlastReceiver receiver = hitCollider.GetComponent<IBlastReceiver>();
            if (receiver != null)
            {
                Debug.Log($"<color=green>[Air Blast]</color> SUCCESS! Sent blast signal to {hitCollider.gameObject.name} with falloff {falloff}");
                receiver.OnBlast(usePoint.position, falloff);
            }
            else
            {
                Debug.Log($"<color=red>[Air Blast]</color> FAILED: {hitCollider.gameObject.name} has no IBlastReceiver component.");
            }
        }
    }
}