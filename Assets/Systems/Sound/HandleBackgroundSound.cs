using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(AudioLowPassFilter))]
public class HandleBackgroundSound : MonoBehaviour
{
    [SerializeField] private AudioZoneChannelSO channel;
    // Remove global muffledCutoff, etc. – they'll come from each zone.
    [SerializeField] private float normalVolume = 1f;
    [SerializeField] private float normalCutoff = 22000f;
    private AudioLowPassFilter lowPass;
    private AudioSource audioSource;  // for volume
    private Coroutine activeFilterTransition;
    private Coroutine activeVolumeTransition;

    private void Awake()
    {
        lowPass = GetComponent<AudioLowPassFilter>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() => channel.OnZoneEvent += OnZoneEvent;
    private void OnDisable() => channel.OnZoneEvent -= OnZoneEvent;

    private void OnZoneEvent(AudioZone zone, bool isEnter)
    {
        if (isEnter)
        {
            // Apply this zone's muffled settings
            float targetCutoff = zone.MuffledCutoff;
            float targetVolume = zone.MuffledVolume;
            float time = zone.TransitionTime;

            StartTransition(targetCutoff, targetVolume, time);
        }
        else
        {

            StartTransition(normalCutoff, normalVolume, zone.TransitionTime);
        }
    }

    private void StartTransition(float targetCutoff, float targetVolume, float duration)
    {
        if (activeFilterTransition != null) StopCoroutine(activeFilterTransition);
        if (activeVolumeTransition != null) StopCoroutine(activeVolumeTransition);
        activeFilterTransition = StartCoroutine(SmoothCutoffChange(targetCutoff, duration));
        activeVolumeTransition = StartCoroutine(SmoothVolumeChange(targetVolume, duration));
    }

private IEnumerator SmoothCutoffChange(float target, float duration)
{
    float start = lowPass.cutoffFrequency;
    float t = 0;
    Debug.Log($"Cutoff START: {start} → {target} over {duration}s");
    while (t < duration)
    {
        t += Time.deltaTime;
        float newCutoff = Mathf.Lerp(start, target, t / duration);
        lowPass.cutoffFrequency = newCutoff;
        Debug.Log($"Cutoff t={t:F3} val={newCutoff:F0}");
        yield return null;
    }
    lowPass.cutoffFrequency = target;
    Debug.Log($"Cutoff END: {lowPass.cutoffFrequency}");
    activeFilterTransition = null;
}

private IEnumerator SmoothVolumeChange(float target, float duration)
{
    float start = audioSource.volume;
    float t = 0;
    Debug.Log($"Volume transition start: {start} → {target} over {duration}s");
    while (t < duration)
    {
        t += Time.deltaTime;
        float factor = t / duration;
        float newVol = Mathf.Lerp(start, target, factor);
        audioSource.volume = newVol;
        Debug.Log($"t={t:F3}, factor={factor:F3}, volume={newVol:F3}");
        yield return null;
    }
    audioSource.volume = target;
    Debug.Log($"Volume transition finished: {audioSource.volume}");
    activeVolumeTransition = null;
}
}