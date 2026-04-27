using UnityEngine;

public interface IProjectile
{
    void Initialize(Vector3 direction, int damage, Transform sourceTransform);
    void Explode();
}
