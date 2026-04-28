using UnityEngine;
// Author: Joshua Henrikson
public class Pusher : MonoBehaviour
{
    private Vector3 startPos;
    private Rigidbody rb;
    
    private enum PushDirections
    {
        zPositive,
        zNegative,
        xPositive,
        xNegative,
        yPositive,
        yNegative
    }

    [SerializeField] private PushDirections pushDirection;
    [SerializeField] private float pushDistance = 5f;
    [SerializeField] private float pushSpeed = 2f;

    private bool pushing = false;
    private Vector3 pushVector = Vector3.forward;
    private Vector3 targetPos;
    private float pushTimer = 0f;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

 void FixedUpdate()
    {
        if (pushing)
        {
            pushTimer += Time.fixedDeltaTime * pushSpeed;
            float t = Mathf.Clamp01(pushTimer);
            
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            
            // 1. Is FixedUpdate actually ticking, and what are the coordinates?
            Debug.Log($"[Pusher] Moving to: {newPos} | t: {t} | Distance: {pushDistance}");

            rb.MovePosition(newPos);

            // Stop pushing when reached target
            if (t >= 1f)
            {
                pushing = false;
                pushTimer = 0f;
                // 2. Did it instantly finish?
                Debug.Log("[Pusher] Push complete."); 
            }
        }
    }
    
    public void OnActivate()
    {
        Debug.Log($"[Pusher] OnActivate was successfully called! Target direction: {pushDirection}");
        switch (pushDirection)
        {
            case PushDirections.zPositive:
                pushVector = Vector3.forward;
                break;
            case PushDirections.zNegative:
                pushVector = Vector3.back;
                break;
            case PushDirections.xNegative:
                pushVector = Vector3.left;
                break;
            case PushDirections.xPositive:
                pushVector = Vector3.right;
                break;
            case PushDirections.yPositive:
                pushVector = Vector3.up;
                break;
            case PushDirections.yNegative:
                pushVector = Vector3.down;
                break;
        }

        targetPos = startPos + pushVector * pushDistance;
        pushing = true;
        pushTimer = 0f;
    }

    public void Reset()
    {
        transform.position = startPos;
        pushing = false;
        pushTimer = 0f;
        rb.linearVelocity = Vector3.zero;
    }
}
