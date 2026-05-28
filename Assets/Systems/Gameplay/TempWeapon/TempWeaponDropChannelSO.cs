using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/TempWeaponDropChannel")]
public class TempWeaponDropChannelSO : ScriptableObject
{
    public UnityAction OnDrop;  // or use a custom event with no args

    public void RaiseDrop()
    {
        OnDrop?.Invoke();
    }
}