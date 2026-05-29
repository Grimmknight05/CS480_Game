using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ZoneActivatedSound : MonoBehaviour
{
    [Header("Channel")]
    [SerializeField] private AudioZoneChannelSO channel;

    [Header("Sound Settings (when INSIDE the zone)")]
    [SerializeField] private float insideVolume = 1f;
    [SerializeField] private float insideCutoff = 22000f; // no filter

    [Header("Sound Settings (when OUTSIDE the zone)")]
    [SerializeField] private float outsideVolume = 0f;
    [SerializeField] private float outsideCutoff = 100f;  // very muffled

    [Header("Transition")]
    [SerializeField] private float transitionTime = 0.5f; // fallback if zone doesn't provide

    private AudioSource audioSource;
    private AudioLowPassFilter lowPass;
    private Coroutine volumeRoutine;
    private Coroutine cutoffRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        
        // Start with "outside" settings
        audioSource.volume = outsideVolume;
        if (lowPass != null) lowPass.cutoffFrequency = outsideCutoff;
    }

    private void OnEnable() => channel.OnZoneEvent += OnZoneEvent;
    private void OnDisable() => channel.OnZoneEvent -= OnZoneEvent;

    private void OnZoneEvent(AudioZone zone, bool isEnter)
    {
        // Use the zone's transitionTime if available, otherwise fallback
        float duration = zone != null ? zone.TransitionTime : transitionTime;

        if (isEnter)
        {
            // Player entered the wind zone → make wind audible
            StartTransition(insideVolume, insideCutoff, duration);
        }
        else
        {
            // Player exited → make wind silent/muffled
            StartTransition(outsideVolume, outsideCutoff, duration);
        }
    }

    private void StartTransition(float targetVolume, float targetCutoff, float duration)
    {
        if (volumeRoutine != null) StopCoroutine(volumeRoutine);
        volumeRoutine = StartCoroutine(SmoothVolumeChange(targetVolume, duration));

        if (lowPass != null)
        {
            if (cutoffRoutine != null) StopCoroutine(cutoffRoutine);
            cutoffRoutine = StartCoroutine(SmoothCutoffChange(targetCutoff, duration));
        }
    }

    private IEnumerator SmoothVolumeChange(float target, float duration)
    {
        float start = audioSource.volume;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        audioSource.volume = target;
        volumeRoutine = null;
    }

    private IEnumerator SmoothCutoffChange(float target, float duration)
    {
        float start = lowPass.cutoffFrequency;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            lowPass.cutoffFrequency = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        lowPass.cutoffFrequency = target;
        cutoffRoutine = null;
    }
}