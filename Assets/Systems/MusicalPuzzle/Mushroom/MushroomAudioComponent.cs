using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Component pattern: isolates the audio responsibility. The Mushroom orchestrator
// holds a cached reference and delegates note playback here.

[RequireComponent(typeof(AudioSource))]
public class MushroomAudioComponent : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void PlayNote(AudioClip clip, float maxDuration)
    {
        if (clip == null || source == null) return;
        source.clip = clip;
        source.loop = false;
        source.Play();
    }

    public void Stop()
    {
        if (source != null && source.isPlaying) source.Stop();
    }
}
