using UnityEngine;

/// <summary>
/// Allows a weapon to know where it should logically aim (the camera) 
/// vs where it should visually fire from (the muzzle).
/// </summary>
public interface IAimContext 
{
    Transform AimSource { get; }
    Transform VisualFirePoint { get; }
}