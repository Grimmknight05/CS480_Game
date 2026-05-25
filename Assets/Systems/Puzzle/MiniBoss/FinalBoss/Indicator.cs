using System.Collections;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public float lifeTime = 2f;          // How long indicator stays on ground
    public float flashSpeed = 0.2f;      // Interval between flashes

    private MeshRenderer meshRenderer;
    private ObjectPool<Indicator> myPool;
    private Coroutine flashRoutine;
    private Coroutine returnRoutine;

    void Awake()
    {
        // Find MeshRenderer in this GameObject or any child
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
            Debug.LogError($"Indicator on {gameObject.name} has no MeshRenderer in its hierarchy!");
    }

    public void Initialize(ObjectPool<Indicator> pool, Vector3 position)
    {
        myPool = pool;
        transform.position = position;

        // Reset visibility (start visible)
        if (meshRenderer != null) meshRenderer.enabled = true;

        // Start flashing coroutine
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());

        // Schedule return to pool after lifetime (safety)
        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnAfterDelay(lifeTime));
    }

    IEnumerator Flash()
    {
        while (true)
        {
            // Wait for half the flash period
            yield return new WaitForSeconds(flashSpeed);

            // Toggle visibility (enables/disables the MeshRenderer)
            if (meshRenderer != null)
                meshRenderer.enabled = !meshRenderer.enabled;
        }
    }

    IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        if (returnRoutine != null) StopCoroutine(returnRoutine);
        myPool?.Return(this);
    }
}