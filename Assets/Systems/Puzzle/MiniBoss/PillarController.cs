/// <summary>
/// Controls a pillar: raising at phase start, lowering when objectives complete.
/// Manages the TurnableStone on top.
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
public class PillarController : MonoBehaviour
{
    [SerializeField] private TurnableStone stone;
    [SerializeField] private float raiseHeight = 5f;
    [SerializeField] private float lowerSpeed = 2f;
    [SerializeField] private float postLowerDelay = 0.5f;
    
    private Vector3 basePosition;
    private Vector3 raisedPosition;
    private Vector3 loweredPosition;
    private bool isLowered;
    
    private Coroutine lowerCoroutine;

    private void Awake()
    {
        basePosition = transform.position;
        raisedPosition = basePosition + Vector3.up * raiseHeight;
        loweredPosition = basePosition;
    }

    /// <summary>
    /// Called at phase start: raise the pillar and stone.
    /// </summary>
    public void RaisePillar()
    {
        isLowered = false;
        
        if (lowerCoroutine != null)
            StopCoroutine(lowerCoroutine);
        
        lowerCoroutine = StartCoroutine(RaisePillarRoutine());
    }

    /// <summary>
    /// Called when phase objective complete: lower the pillar.
    /// Player can now interact with the stone.
    /// </summary>
    public void LowerPillar()
    {
        if (isLowered) return;
        
        isLowered = true;
        
        if (lowerCoroutine != null)
            StopCoroutine(lowerCoroutine);
        
        lowerCoroutine = StartCoroutine(LowerPillarRoutine());
    }

    /// <summary>
    /// Enable the stone for rotation.
    /// </summary>
    public void EnableStone()
    {
        if (stone != null)
        {
            stone.SetInputLock(false);
            stone.SetPlayerLock(false);
        }
    }

    /// <summary>
    /// Disable the stone temporarily.
    /// </summary>
    public void DisableStone()
    {
        if (stone != null)
        {
            stone.SetPlayerLock(true);
        }
    }

    private IEnumerator RaisePillarRoutine()
    {
        EnableStone();
        
        while (Vector3.Distance(transform.position, raisedPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                raisedPosition,
                Time.deltaTime * lowerSpeed
            );
            yield return null;
        }
        
        transform.position = raisedPosition;
    }

    private IEnumerator LowerPillarRoutine()
    {
        // Play lower animation
        while (Vector3.Distance(transform.position, loweredPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                loweredPosition,
                Time.deltaTime * lowerSpeed
            );
            yield return null;
        }
        
        transform.position = loweredPosition;
        yield return new WaitForSeconds(postLowerDelay);
        
        // Stone is now accessible
        EnableStone();
    }

    public void Cleanup()
    {
        if (lowerCoroutine != null)
            StopCoroutine(lowerCoroutine);
    }

    public TurnableStone GetStone() => stone;
}
