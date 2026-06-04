using UnityEngine;

// Persistent singleton that plays the shared UI button-click SFX.
//
// Place one instance in the first scene (MainMenu) with an AudioSource
// whose clip is set to the desired click sound.  It persists via
// DontDestroyOnLoad so every subsequent scene can call UIClickSound.Play()
// without needing their own AudioClip reference.
//
// Usage anywhere:
//   UIClickSound.Play();
//
// Added by: Katie Trinh

[RequireComponent(typeof(AudioSource))]
public class UIClickSound : MonoBehaviour
{
    public static UIClickSound Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Play the UI click SFX once.  Safe to call before the singleton is
    /// fully initialised — the call is silently ignored.
    /// </summary>
    public static void Play()
    {
        if (Instance == null || Instance.audioSource == null) return;
        if (Instance.audioSource.clip == null) return;
        Instance.audioSource.PlayOneShot(Instance.audioSource.clip);
    }
}
