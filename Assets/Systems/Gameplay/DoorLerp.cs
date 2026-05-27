using System.Collections;
using UnityEngine;

public class DoorLerp : ResettableBehaviour
{
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 4f, 0f);
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private ParticleSystem openVFX;
    [SerializeField] private bool debugMode;
    [SerializeField] private DoorLerp[] linkedDoorsToForceOpenOnForceClose;
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Coroutine running;
    private bool isOpen;

    void Awake()
    {
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + openOffset;
        Log($"Awake closedLocalPos={closedLocalPos} openLocalPos={openLocalPos} openOffset={openOffset}");
    }
    
    public void Open()
    {
        if (isOpen)
        {
            Log($"Open ignored because isOpen is already true. currentLocalPos={transform.localPosition}");
            return;
        }

        isOpen = true;
        if (openVFX != null) openVFX.Play();
        Log($"Open -> {openLocalPos}");
        StartLerp(openLocalPos);
    }

    public void Close()
    {
        if (!isOpen)
        {
            Log($"Close ignored because isOpen is already false. currentLocalPos={transform.localPosition}");
            return;
        }

        isOpen = false;
        Log($"Close -> {closedLocalPos}");
        StartLerp(closedLocalPos);
    }

    public void ForceOpen()
    {
        isOpen = true;
        if (openVFX != null) openVFX.Play();
        Log($"ForceOpen -> {openLocalPos}");
        StartLerp(openLocalPos);
    }

    public void ForceClose()
    {
        isOpen = false;
        Log($"ForceClose -> {closedLocalPos}");
        StartLerp(closedLocalPos, ForceOpenLinkedDoors);
    }

    void StartLerp(Vector3 target, System.Action onComplete = null)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(LerpTo(target, onComplete));
    }

    IEnumerator LerpTo(Vector3 target, System.Action onComplete)
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
        Log($"Reached target {target}");
        onComplete?.Invoke();
    }

    private void ForceOpenLinkedDoors()
    {
        if (linkedDoorsToForceOpenOnForceClose == null) return;

        for (int i = 0; i < linkedDoorsToForceOpenOnForceClose.Length; i++)
        {
            DoorLerp linkedDoor = linkedDoorsToForceOpenOnForceClose[i];
            if (linkedDoor == null) continue;
            Log($"Force opening linked door {linkedDoor.name}");
            linkedDoor.ForceOpen();
        }
    }

    private void Log(string message)
    {
        if (!debugMode) return;
        Debug.Log($"[DoorLerp:{name}] {message}", this);
    }

    protected override void ResetInternal()
    {
        if (isOpen) Close();
    }
}
