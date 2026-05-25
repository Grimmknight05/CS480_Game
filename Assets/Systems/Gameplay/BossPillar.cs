using System.Collections;
using UnityEngine;
public class BossPillar : ResettableBehaviour
{
    //Adapted from David's door code
    [SerializeField] private Vector3 maxOffset = new Vector3(0f, 15f, 0f);
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private ParticleSystem moveVFX;

    private Vector3 startLocalPos;
    private Vector3 maxLocalPos;
    private Coroutine running;
    private bool isMax;
    private bool isRaised;

    void Awake()
    {
        startLocalPos = transform.localPosition;
        maxLocalPos = startLocalPos + maxOffset;
    }

    public void MaxHeight()
    {
        if (isMax) return;
        isMax = true;
        isRaised = true;
        if (moveVFX != null) moveVFX.Play();
        StartLerp(maxLocalPos);
    }
    public void PercentHeight(float percentage)
    {
        isRaised = true;
        isMax = false;
        if (moveVFX != null) moveVFX.Play();
        Vector3 target = Vector3.Lerp(startLocalPos, maxLocalPos, percentage);
        StartLerp(target);
        
    }

    public void Reset()
    {
        if (!isMax || !isRaised) return;
        isMax = false;
        isRaised = false;
        StartLerp(startLocalPos);
    }

    void StartLerp(Vector3 target)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(LerpTo(target));
    }

    IEnumerator LerpTo(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t / duration));
            transform.localPosition = Vector3.Lerp(start, target, k);
            yield return null;
        }
        
        transform.localPosition = target;
        running = null;
    }
    protected override void ResetInternal()
    {
        Reset();
    }
}
