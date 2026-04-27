using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    public float duration;

    public abstract void Apply(GameObject target, Vector3 hitDirection);
}