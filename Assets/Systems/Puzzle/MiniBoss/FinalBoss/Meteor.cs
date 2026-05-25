using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 10f;
    public int damageAmount = 25;
    private ObjectPool<Meteor> myPool;   // reference to the pool that owns this meteor

    private Vector3 targetGroundPoint;
    private bool isFalling;

    public void Initialize(ObjectPool<Meteor> pool, Vector3 startPos)
    {
        myPool = pool;
        transform.position = startPos;
        targetGroundPoint = new Vector3(startPos.x, 0, startPos.z);
        isFalling = true;
    }

    void Update()
    {
        if (!isFalling) return;
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        if (transform.position.y <= targetGroundPoint.y)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damageAmount);
        
        ReturnToPool();
    }

    void ReturnToPool()
    {
        isFalling = false;
        myPool?.Return(this);
    }
}