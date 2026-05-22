using UnityEngine;

[CreateAssetMenu(fileName = "TempNodeWeapon", menuName = "Tools/Temp Node Weapon")]
public class TempNodeWeapon : Weapon
{
    [Header("Node Weapon Specific")]
    [SerializeField] private float range = 30f;
    [SerializeField] private GameObject hitEffect;
    
    private NodeDestructionPhase currentPhase;
    
    public void SetCurrentPhase(NodeDestructionPhase phase)
    {
        currentPhase = phase;
    }
    
    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {
        if (currentPhase == null) return;
        
        // Raycast to find node
        Ray ray = new Ray(usePoint.position, usePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, targetLayer))
        {
            var nodeObject = hit.collider.gameObject;
            if (currentPhase.TryDamageNode(nodeObject))
            {
                // Successful hit
                PlayUseSound(audioSource);
                
                if (hitEffect != null)
                    Instantiate(hitEffect, hit.point, Quaternion.identity);
                
                Debug.Log("Node damaged!");
            }
        }
    }
}