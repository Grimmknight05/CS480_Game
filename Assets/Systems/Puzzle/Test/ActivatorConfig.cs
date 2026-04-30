public abstract class ActivatorConfig : UnityEngine.ScriptableObject
{
    public abstract IReactiveRequirement[] CreateRequirements();
}