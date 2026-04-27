using UnityEngine;

[CreateAssetMenu(fileName = "RaycastWeapon", menuName = "Weapons/Raycast Weapon")]
public class RaycastWeapon : Weapon
{
    [Header("Raycast Settings")]
    [SerializeField] public float rayDistance = 100f;
    [SerializeField] private LineRenderer lineRenderer;

    public override void Use(Transform firePoint, AudioSource audioSource, LayerMask layerMask)
    {

        PlayUseSound(audioSource);

        RaycastHit hit;
        Vector3 rayOrigin = firePoint.position;
        Vector3 rayDirection = firePoint.forward;

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * rayDistance);
        }

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, layerMask))
        {
            Debug.Log($"[RaycastWeapon] Hit: {hit.collider.gameObject.name}");

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyControllerTest enemy = hit.collider.GetComponent<EnemyControllerTest>();
                if (enemy != null)
                {
                    Debug.Log($"[RaycastWeapon] Dealt {damagePerHit} damage!");
                    enemy.TakeDamage(damagePerHit);
                }
            }
        }
    }
}
