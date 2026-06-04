using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent background music player.
//
// DontDestroyOnLoad singleton — plays only while in the Main Menu or Space Hub.
// Automatically pauses when a level scene loads and resumes on return.
// This prevents music from layering when the player enters / exits levels.
//
// Volume is driven by AudioListener.volume (set via the Settings screen).
//
// Added by: Katie Trinh

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // Scenes where the hub soundtrack should be audible.
    private static readonly string[] s_MusicScenes = { "MainMenu", "J'sSpaceHub" };

    private AudioSource audioSource;

    private void Awake()
    {
        // Destroy duplicates — the first instance wins.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        // Disable Unity's built-in auto-play; we drive playback entirely
        // from OnSceneLoaded so there is no race with the singleton guard.
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Play immediately if the very first scene is a music scene.
        HandleScene(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene.name);
    }

    private void HandleScene(string sceneName)
    {
        if (audioSource == null) return;

        bool isMusicScene = System.Array.IndexOf(s_MusicScenes, sceneName) >= 0;

        if (isMusicScene)
        {
            if (!audioSource.isPlaying && audioSource.clip != null)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
