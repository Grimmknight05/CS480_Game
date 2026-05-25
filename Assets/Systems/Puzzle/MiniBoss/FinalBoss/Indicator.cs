using System.Collections;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public float lifeTime = 2f;
    public float flashSpeed = 0.2f;
    [SerializeField] SpriteRenderer sprite;
    private ObjectPool<Indicator> myPool;
    private Coroutine flashRoutine;
    private Coroutine returnRoutine;

    public void Initialize(ObjectPool<Indicator> pool, Vector3 position)
    {
        myPool = pool;
        transform.position = position;
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite == null)
            Debug.LogError($"Indicator on {gameObject.name} has no SpriteRenderer in its hierarchy!");

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());

        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnAfterDelay(lifeTime));
    }

    IEnumerator Flash()
    {
        while (true)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(flashSpeed);
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