using UnityEngine;
using UnityEngine.InputSystem;
//Deprecated
public class RaycastDamage : MonoBehaviour
{
    [SerializeField] private float rayDistance = 100f; // How far the raycast travels
    [SerializeField] private int damagePerHit = 20; // Damage dealt per successful hit
    [SerializeField] private float fireCooldown = 0.5f; // Cooldown between shots in seconds
    [SerializeField] private LayerMask enemyLayer; // Layer mask for enemies
    [SerializeField] private Transform firePoint; // Where the raycast originates from (optional)
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSFX;
    [SerializeField] private LineRenderer lineRenderer;

    private float lastFireTime = 0f;
    private PlayerInput playerInput;
    private InputAction attackAction;
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        // If no firePoint specified, use this transform
        if (firePoint == null)
        {
            firePoint = transform;
        }
        // Get PlayerInput component
        /*if (playerInput != null)
        {
            // Find the Attack action
            attackAction = playerInput.actions.FindAction("Attack");
            if (attackAction != null)
            {
                attackAction.performed += OnAttack;
            }
        }*/
    }


    // Called when the Attack input is performed
    void OnAttack(InputValue attackInput)
    {
        if (attackInput.isPressed)
        {
            Fire();
        }
    }


    // Fire a raycast and damage any enemy hit
    public void Fire()
    {
        // Check cooldown
        if (Time.time - lastFireTime < fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;

        // Cast ray
        RaycastHit hit;
        Vector3 rayOrigin = firePoint.position;
        Vector3 rayDirection = firePoint.forward;

        // Play fire sound
        if (fireSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSFX);
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * rayDistance);
        }

        // Check if we hit something on the enemy layer
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");

            // Check if the hit object is an enemy by tag
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyControllerTest enemy = hit.collider.GetComponent<EnemyControllerTest>();
                if (enemy != null)
                {
                    Debug.Log("Dealing damage! Enemy!");
                    //enemy.TakeDamage(damagePerHit);
                }
                else
                {
                    Debug.LogWarning("Enemy tag found but no EnemyControllerTest component!");
                }
            }
            else
            {
                Debug.Log($"Hit object is not tagged as Enemy, tag is: {hit.collider.tag}");
            }
        }
    }

    public void FireInDirection(Vector3 direction)
    {
        if (Time.time - lastFireTime < fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;

        RaycastHit hit;
        Vector3 rayOrigin = firePoint.position;
        Vector3 rayDirection = direction.normalized;

        if (fireSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSFX);
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * rayDistance);
        }

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyControllerTest enemy = hit.collider.GetComponent<EnemyControllerTest>();
                if (enemy != null)
                {
                    Debug.Log("Dealing damage!");
                    //enemy.TakeDamage(damagePerHit);
                }
                else
                {
                    Debug.LogWarning("Enemy tag found but no EnemyControllerTest component!");
                }

            }
            else
            {
                Debug.Log($"Hit object is not tagged as Enemy, tag is: {hit.collider.tag}");
            }
        }
    }

    // Getters and setters
    public void SetDamage(int newDamage) => damagePerHit = newDamage;
    public void SetCooldown(float newCooldown) => fireCooldown = newCooldown;
    public void SetRayDistance(float newDistance) => rayDistance = newDistance;
}
