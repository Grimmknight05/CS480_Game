using System.Collections;
using UnityEngine;

public class DoorLerp : MonoBehaviour, IResettable
{
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 4f, 0f);
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private ParticleSystem openVFX;
    [SerializeField] private LevelResetChannelSO resetChannel;
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Coroutine running;
    private bool isOpen;
    private void OnEnable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised += ResetState;
    }

    private void OnDisable()
    {
        if (resetChannel != null)
            resetChannel.OnRaised -= ResetState;
    }
    public void ResetState()   // ← interface method implementation
    {
        if (isOpen)
            Close();
    }
    void Awake()
    {
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + openOffset;
    }
    
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        if (openVFX != null) openVFX.Play();
        StartLerp(openLocalPos);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        StartLerp(closedLocalPos);
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
}
