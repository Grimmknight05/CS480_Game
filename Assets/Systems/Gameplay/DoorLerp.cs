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
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip moveLoopSound;
    [SerializeField] private AudioClip closeSound; 
    [SerializeField] private AudioClip atPosSound; 
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Coroutine running;
    private bool isOpen;

    void Awake()
    {
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + openOffset;
        Log($"Awake closedLocalPos={closedLocalPos} openLocalPos={openLocalPos} openOffset={openOffset}");
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound by default, adjust as needed
            }
        }
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
        PlaySound(openSound);
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
        PlaySound(closeSound);
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
        if (running != null)
        {
            StopCoroutine(running);
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();
        }
        running = StartCoroutine(LerpTo(target, onComplete));
    }

    IEnumerator LerpTo(Vector3 target, System.Action onComplete)
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
        Log($"Reached target {target}");
        PlaySound(atPosSound);
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
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }

    protected override void ResetInternal()
    {
        if (isOpen) Close();
    }

    // Unconditionally return the door to its closed state and clear isOpen, with no
    // linked-door side effects. Used by the controller that opened it (e.g. a
    // DialogueTrigger) to guarantee a clean pre-trigger state on level reset, even if
    // isOpen has desynced or this door's own reset channel/area gate skipped it.
    public void ResetToClosed()
    {
        isOpen = false;
        Log($"ResetToClosed -> {closedLocalPos}");
        StartLerp(closedLocalPos);
    }
}
