// =====================================================================
// Passive view component for the dialogue system. It does NOT decide
// what to show or when — it only exposes Show / Hide / SetSpeaker /
// SetLine / SetContinueIndicatorVisible / PlayBlip methods that the
// DialogueRunner calls.
//
// Attach this to a Canvas (or a panel under a Canvas) and wire the
// TextMeshProUGUI / Image fields in the Inspector.
// =====================================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("Root GameObject toggled on/off when a conversation starts/ends. " +
             "If left empty, this component's GameObject is used.")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Optional")]
    [Tooltip("Optional portrait image. Hidden if the speaker has no portrait.")]
    [SerializeField] private Image portraitImage;

    [Tooltip("Optional 'press to continue' indicator (e.g. blinking arrow). " +
             "Toggled on when the current bubble has finished revealing.")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Audio")]
    [Tooltip("Optional audio source used for typing blips.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Style \u2014 Font")]
    [Tooltip("Optional TMP font asset applied to both speaker and body text. " +
             "Leave empty to keep the default TMP font.")]
    [SerializeField] private TMP_FontAsset font;

    [Tooltip("Font size for the speaker name.")]
    [SerializeField] private float speakerFontSize = 32f;

    [Tooltip("Font size for the body text.")]
    [SerializeField] private float bodyFontSize = 24f;

    [Tooltip("Font style flags applied to the speaker name (Bold, Italic, etc.).")]
    [SerializeField] private FontStyles speakerFontStyle = FontStyles.Bold;

    [Tooltip("Font style flags applied to the body text.")]
    [SerializeField] private FontStyles bodyFontStyle = FontStyles.Normal;

    [Tooltip("Default color for the body text. (Speaker name color comes from the speaker asset.)")]
    [SerializeField] private Color bodyColor = Color.white;

    [Tooltip("Extra spacing between characters in the body text.")]
    [SerializeField] private float bodyCharacterSpacing = 0f;

    [Tooltip("Extra spacing between lines in the body text.")]
    [SerializeField] private float bodyLineSpacing = 0f;

    [Header("Style \u2014 Outline")]
    [Tooltip("If true, gives both texts a colored outline (helps readability over busy backgrounds).")]
    [SerializeField] private bool useOutline = true;
    [SerializeField] private Color outlineColor = Color.black;
    [Range(0f, 1f)]
    [SerializeField] private float outlineWidth = 0.2f;

    [Header("Style \u2014 Vertex Gradient (speaker name)")]
    [Tooltip("If true, the speaker name uses a top-to-bottom color gradient " +
             "instead of a flat color. Falls back to NPCSpeakerSO.nameColor when off.")]
    [SerializeField] private bool useSpeakerGradient = false;
    [SerializeField] private Color speakerGradientTop = Color.white;
    [SerializeField] private Color speakerGradientBottom = new Color(0.6f, 0.6f, 0.6f, 1f);

    void Awake()
    {
        if (root == null) root = gameObject;
        ApplyStyle();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Live-preview style tweaks in the editor without entering Play mode.
        if (!Application.isPlaying) ApplyStyle();
    }
#endif

    /// <summary>
    /// Applies the configured style to the speaker name and body text.
    /// Called automatically on Awake / OnValidate; can be called manually
    /// after changing fields at runtime.
    /// </summary>
    public void ApplyStyle()
    {
        ApplyStyleTo(speakerNameText, isSpeaker: true);
        ApplyStyleTo(bodyText, isSpeaker: false);
    }

    private void ApplyStyleTo(TextMeshProUGUI text, bool isSpeaker)
    {
        if (text == null) return;

        if (font != null) text.font = font;

        text.fontSize = isSpeaker ? speakerFontSize : bodyFontSize;
        text.fontStyle = isSpeaker ? speakerFontStyle : bodyFontStyle;

        if (!isSpeaker)
        {
            text.color = bodyColor;
            text.characterSpacing = bodyCharacterSpacing;
            text.lineSpacing = bodyLineSpacing;
            text.enableVertexGradient = false;
        }
        else
        {
            text.enableVertexGradient = useSpeakerGradient;
            if (useSpeakerGradient)
            {
                text.colorGradient = new VertexGradient(
                    speakerGradientTop, speakerGradientTop,
                    speakerGradientBottom, speakerGradientBottom);
            }
        }

        // outlineColor / outlineWidth instance the font material so each text
        // can have its own outline. Safe for a single UI panel.
        if (useOutline)
        {
            text.outlineColor = outlineColor;
            text.outlineWidth = outlineWidth;
        }
        else
        {
            text.outlineWidth = 0f;
        }

        // Force TMP to rebuild its mesh now. Without this, font/material
        text.SetAllDirty();
        text.ForceMeshUpdate(true, true);
    }

    public void Show()
    {
        // Lazy-init for the case where the panel starts disabled in the scene
        // (Awake doesn't run on disabled GameObjects, so root could still be null here).
        if (root == null) root = gameObject;
        root.SetActive(true);
        SetContinueIndicatorVisible(false);
    }

    public void Hide()
    {
        if (root == null) root = gameObject;
        SetContinueIndicatorVisible(false);
        root.SetActive(false);
    }

    public void SetSpeaker(NPCSpeakerSO speaker)
    {
        if (speakerNameText != null)
        {
            if (speaker != null)
            {
                speakerNameText.text = speaker.DisplayName;
                speakerNameText.color = speaker.NameColor;
            }
            else
            {
                speakerNameText.text = string.Empty;
            }
        }

        if (portraitImage != null)
        {
            if (speaker != null && speaker.Portrait != null)
            {
                portraitImage.sprite = speaker.Portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }
    }

    public void SetLine(string text)
    {
        if (bodyText != null) bodyText.text = text ?? string.Empty;
    }

    public void SetContinueIndicatorVisible(bool visible)
    {
        if (continueIndicator != null) continueIndicator.SetActive(visible);
    }

    public void PlayBlip(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}
