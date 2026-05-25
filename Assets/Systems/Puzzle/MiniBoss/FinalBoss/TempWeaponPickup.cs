using UnityEngine;

public class TempWeaponPickup : MonoBehaviour
{
    private TempNodeWeapon weapon;
    private NodeDestructionPhase phase;
    private bool collected = false;
    
    public void Init(TempNodeWeapon w, NodeDestructionPhase p)
    {
        weapon = w;
        phase = p;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        var tools = other.GetComponent<ToolSystem>();
        if (tools != null && weapon != null)
        {
            weapon.SetCurrentPhase(phase);
            tools.AddTemporaryWeapon(weapon);
            phase.OnWeaponPickedUp(this, tools);
        }
        Destroy(gameObject);
    }
}