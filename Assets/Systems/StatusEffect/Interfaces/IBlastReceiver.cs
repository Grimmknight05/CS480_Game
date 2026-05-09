using UnityEngine;

public interface IBlastReceiver 
{
    // normalizedFalloff is 1 at the center of the blast, 0 at the extreme edge.
    void OnBlast(Vector3 origin, float normalizedFalloff);
}