// =====================================================================
// Defines a reusable speaker asset for the dialogue system.
// One NPCSpeakerSO represents "who is talking" (display name, portrait,
// name color, optional voice blip). Multiple DialogueSO conversations
// can reference the same speaker so changes (e.g. renaming a character)
// only need to happen in one place.
// =====================================================================

using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/NPC Speaker", fileName = "NewNPCSpeaker")]
public class NPCSpeakerSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Name shown above the dialogue text in the UI.")]
    [SerializeField] private string displayName = "Unknown";

    [Tooltip("Optional portrait shown next to the dialogue. Leave empty if not used.")]
    [SerializeField] private Sprite portrait;

    [Tooltip("Color applied to the speaker's display name in the UI.")]
    [SerializeField] private Color nameColor = Color.white;

    [Header("Audio (optional)")]
    [Tooltip("Optional 'blip' sound played as text reveals. Leave empty for no sound.")]
    [SerializeField] private AudioClip typingBlip;

    [Tooltip("Volume multiplier for the typing blip.")]
    [Range(0f, 1f)]
    [SerializeField] private float typingBlipVolume = 0.5f;

    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public Color NameColor => nameColor;
    public AudioClip TypingBlip => typingBlip;
    public float TypingBlipVolume => typingBlipVolume;
}
