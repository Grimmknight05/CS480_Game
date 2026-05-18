using UnityEngine;

/// <summary>
/// Audio counterpart to <see cref="MaterialController"/>: a list of slots with passive/active clips and booleans,
/// wired from UnityEvents (e.g. PuzzleValidator On Solved / On Unsolved) just like materials.
/// </summary>
public class SoundToggleController : MonoBehaviour, ILock
{
    [System.Serializable]
    public class SoundFlip
    {
        [Tooltip("Optional. Leave empty to play at SoundToggleController transform position.")]
        public AudioSource source;
        public AudioClip passiveClip;
        public AudioClip activeClip;
        public bool state;
    }

    [SerializeField] public SoundFlip[] Object;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private bool _lockState;
    public bool LockState
    {
        get => _lockState;
        set => _lockState = value;
    }

    public void Lock(bool lockstate)
    {
        LockState = lockstate;
        Debug.Log($"{nameof(SoundToggleController)} {(lockstate ? "LOCKED" : "UNLOCKED")}");
    }

    /// <summary>No playback — avoids firing clips on scene load. Slot <c>state</c> still comes from the Inspector.</summary>
    public void InitAllSounds() { }

    public void ToggleAllSounds()
    {
        IsLocked();
        foreach (SoundFlip ff in Object)
        {
            if (ff == null) continue;
            bool was = ff.state;
            ff.state = !ff.state;
            if (was != ff.state)
                PlayForState(ff);
        }
    }

    public void SetAllSounds(bool state)
    {
        IsLocked();
        foreach (SoundFlip ff in Object)
        {
            if (ff == null) continue;
            bool was = ff.state;
            ff.state = state;
            if (was != ff.state)
                PlayForState(ff);
        }
    }

    public void SetAllSoundsAndLock(bool state)
    {
        IsLocked();
        foreach (SoundFlip ff in Object)
        {
            if (ff == null) continue;
            bool was = ff.state;
            ff.state = state;
            if (was != ff.state)
                PlayForState(ff);
        }
        _lockState = true;
    }

    /// <summary>MaterialController reapplies visuals; audio has nothing to refresh without replaying — left empty on purpose.</summary>
    public void UpdateAllSounds() { }

    public void ToggleSound(int index)
    {
        IsLocked();
        if (index < 0 || index >= Object.Length) return;

        SoundFlip ff = Object[index];
        if (ff == null) return;

        bool was = ff.state;
        ff.state = !ff.state;
        if (was != ff.state)
            PlayForState(ff);
    }

    public void SetSound(int index, bool state)
    {
        IsLocked();
        if (index < 0 || index >= Object.Length) return;

        SoundFlip ff = Object[index];
        if (ff == null) return;

        bool was = ff.state;
        ff.state = state;
        if (was != ff.state)
            PlayForState(ff);
    }

    public void ResetAllSounds()
    {
        IsLocked();
        foreach (SoundFlip ff in Object)
        {
            if (ff == null) continue;
            ff.state = false;
        }
    }

    public void ResetAllSoundsBypassLock()
    {
        foreach (SoundFlip ff in Object)
        {
            if (ff == null) continue;
            ff.state = false;
        }
    }

    private void PlayForState(SoundFlip ff)
    {
        AudioClip clip = ff.state ? ff.activeClip : ff.passiveClip;
        if (clip == null) return;

        if (ff.source != null)
            ff.source.PlayOneShot(clip, volume);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }

    private void IsLocked()
    {
        if (LockState)
            Debug.LogWarning($"{nameof(SoundToggleController)} is locked!");
    }

    private void OnDisable()
    {
        ResetAllSoundsBypassLock();
    }
}
