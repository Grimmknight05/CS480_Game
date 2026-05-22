using UnityEngine;

public class FloatingNode : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 1.5f;
    
    [Header("Shield Visual")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private Material damagedMaterial;
    [SerializeField] private float damagedFlashDuration = 0.2f;
    
    private Vector3 startPosition;
    private float currentShieldHealth = 1f;
    private Material originalMaterial;
    private Renderer shieldRenderer;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (shieldVisual != null)
        {
            shieldRenderer = shieldVisual.GetComponent<Renderer>();
            if (shieldRenderer != null)
                originalMaterial = shieldRenderer.material;
        }
    }
    
    void Update()
    {
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
    
    public void UpdateShieldHealth(float healthPercent)
    {
        currentShieldHealth = healthPercent;
        
        if (shieldRenderer != null && damagedMaterial != null)
        {
            StartCoroutine(FlashDamaged());
        }
    }
    
    private System.Collections.IEnumerator FlashDamaged()
    {
        shieldRenderer.material = damagedMaterial;
        yield return new WaitForSeconds(damagedFlashDuration);
        shieldRenderer.material = originalMaterial;
    }
}