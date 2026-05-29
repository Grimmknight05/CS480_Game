using System.Collections;
using UnityEngine;
public class BossPillar : ResettableBehaviour
{
    //Adapted from David's door code
    [SerializeField] private PillarSO identifier;
    [SerializeField] private Vector3 maxOffset = new Vector3(0f, 15f, 0f);
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private ParticleSystem moveVFX;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveLoopSound;
    [SerializeField] private AudioClip destinationSound;
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
        protected override void OnEnable()
    {
        base.OnEnable();  // ← this subscribes to resetChannel

        if (identifier != null)
            PillarRegistry.Register(identifier, this);
    }

    protected override void OnDisable()
    {
        base.OnDisable(); // ← this unsubscribes from resetChannel

        if (identifier != null)
            PillarRegistry.Unregister(identifier);
    }
    public void MaxHight()
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
        if (running != null)
        {
            StopCoroutine(running);
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
        }
        running = StartCoroutine(LerpTo(target));
    }

    IEnumerator LerpTo(Vector3 target)
    {
        if (audioSource != null && moveLoopSound != null)
        {
            audioSource.clip = moveLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
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

        // Stop looping movement sound
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == moveLoopSound)
            audioSource.Stop();

        // Play destination reached sound
        if (audioSource != null && destinationSound != null)
            audioSource.PlayOneShot(destinationSound);
    }
    protected override void ResetInternal()
    {
        Reset();
    }
}
