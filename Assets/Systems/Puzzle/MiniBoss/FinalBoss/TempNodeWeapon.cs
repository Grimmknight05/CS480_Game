using UnityEngine;

[CreateAssetMenu(fileName = "TempNodeWeapon", menuName = "Tools/Temp Node Weapon")]
public class TempNodeWeapon : Weapon
{
    [Header("Node Weapon Specific")]
    [SerializeField] private GameObject hitEffect;

    private NodeDestructionPhase currentPhase;

    public void SetCurrentPhase(NodeDestructionPhase phase)
    {
        currentPhase = phase;
    }

    public override void Use(Transform usePoint, AudioSource audioSource, LayerMask layerMask)
    {
        var phase = NodeDestructionPhase.Instance;
        if (phase == null) return;
        
        Ray ray = new Ray(usePoint.position, usePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, layerMask))
        {
            if (phase.TryDamageNode(hit.collider.gameObject))
            {
                PlayUseSound(audioSource);
                if (hitEffect != null)
                    Instantiate(hitEffect, hit.point, Quaternion.identity);
            }
        }
    }

    protected override bool IsValidTarget(Collider candidate)
    {
        return candidate.GetComponent<FloatingNode>() != null;
    }
}