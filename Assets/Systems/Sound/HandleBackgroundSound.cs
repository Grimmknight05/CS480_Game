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
            float targetCutoff = zone.MuffledVolume;
            float targetVolume = zone.MuffledVolume;
            float time = zone.TransitionTime;

            StartTransition(targetCutoff, targetVolume, time);
        }
        else
        {
            // Exit – restore normal settings (could also use zone's normalCutoff/Volume if each zone defines them)
            float targetCutoff = normalCutoff;      // you may still keep global normal values
            float targetVolume = normalVolume;      // or read from zone.normalCutoff etc.
            float time = zone.TransitionTime;

            StartTransition(targetCutoff, targetVolume, time);
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
        while (t < duration)
        {
            t += Time.deltaTime;
            lowPass.cutoffFrequency = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        lowPass.cutoffFrequency = target;
        activeFilterTransition = null;
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
        activeVolumeTransition = null;
    }
}