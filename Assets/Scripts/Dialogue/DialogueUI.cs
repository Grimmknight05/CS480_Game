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
    [SerializeField] private TMP_Text  speakerNameText;
    [SerializeField] private TMP_Text  bodyText;

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
    [SerializeField] private float speakerFontSize = 60f;

    [Tooltip("Font size for the body text.")]
    [SerializeField] private float bodyFontSize = 50f;

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
    [SerializeField] private Material outlineMaterial;
    [Range(0f, 1f)]
    [SerializeField] private float outlineWidth = 0.2f;

    [Header("Style \u2014 Vertex Gradient (speaker name)")]
    [Tooltip("If true, the speaker name uses a top-to-bottom color gradient " +
             "instead of a flat color. Falls back to NPCSpeakerSO.nameColor when off.")]
    [SerializeField] private bool useSpeakerGradient = false;
    [SerializeField] private Color speakerGradientTop = Color.white;
    [SerializeField] private Color speakerGradientBottom = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Style — Auto Size")]
    [Tooltip("When true, the speaker/body font sizes above are treated as the MAXIMUM size and TMP " +
             "auto-sizing shrinks long lines so they never spill out of the text box.")]
    [SerializeField] private bool enableAutoSize = true;

    [Tooltip("Minimum auto-size as a fraction of the configured (max) font size. " +
             "0.6 means text can shrink to 60% before it word-wraps / truncates.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float autoSizeMinFactor = 0.6f;

    [Header("Canvas Scaling")]
    [Tooltip("If true, forces the parent Canvas Scaler to 'Scale With Screen Size' at runtime so " +
             "dialogue text renders at a consistent relative size in the editor, fullscreen, and builds. " +
             "Fixes the 'huge in a small window / tiny in a build' bug for every NPC sharing this panel. " +
             "This affects the entire parent Canvas (e.g. the HUD), which is the intended behavior.")]
    [SerializeField] private bool enforceCanvasScaler = true;

    [Tooltip("Reference resolution the parent Canvas Scaler is forced to when 'Enforce Canvas Scaler' is on.")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Tooltip("Match-width-or-height blend for the forced Canvas Scaler (0 = width, 1 = height).")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    void Awake()
    {
        if (root == null) root = gameObject;
        EnforceCanvasScaler();
        ApplyStyle();
    }

    /// <summary>
    /// Forces the parent Canvas Scaler into "Scale With Screen Size" mode at runtime.
    /// Runtime-only on purpose: we never rewrite the serialized scene/prefab asset, so this
    /// causes no git churn and respects the Scene-Freeze workflow.
    /// </summary>
    private void EnforceCanvasScaler()
    {
        if (!enforceCanvasScaler) return;

        CanvasScaler scaler = GetComponentInParent<CanvasScaler>(true);
        if (scaler == null) return;

        scaler.uiScaleMode      = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode  = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;
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

    private void ApplyStyleTo(TMP_Text  text, bool isSpeaker)
    {
        if (text == null) return;
        if (text.font == null)
        {
            Debug.LogWarning($"{text.name} has no font asset – skipping style", text);
            return;
        }

        if (font != null) text.font = font;

        float targetSize = isSpeaker ? speakerFontSize : bodyFontSize;
        if (enableAutoSize)
        {
            // Treat the configured size as the MAX; let TMP shrink long lines to fit the box.
            text.enableAutoSizing = true;
            text.fontSizeMax = targetSize;
            text.fontSizeMin = targetSize * autoSizeMinFactor;
            // Belt-and-suspenders: even at min size, wrap and clip rather than spill past the box.
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Truncate;
        }
        else
        {
            text.enableAutoSizing = false;
            text.fontSize = targetSize;
        }
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
        if (useOutline && outlineMaterial != null)
        {
            // Create a unique instance for this text so we can adjust color/width per text
            Material mat = new Material(outlineMaterial);
            mat.SetColor("_OutlineColor", outlineColor);
            mat.SetFloat("_OutlineWidth", outlineWidth);
            text.fontMaterial = mat;
        }
        else if (!useOutline)
        {
            // Revert to the font's default material (no outline)
            text.fontMaterial = text.font.material;
        }

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
                speakerNameText.color = useSpeakerGradient ? Color.white : speaker.NameColor;
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

    /// <summary>
    /// Sets the full body text once and forces a mesh update so auto-sizing settles on its
    /// final point size immediately. Pair with <see cref="SetVisibleCharacters"/> for a
    /// typewriter reveal that does NOT resize/jitter as characters appear.
    /// </summary>
    public void SetLineInstant(string text)
    {
        if (bodyText == null) return;
        bodyText.text = text ?? string.Empty;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();
    }

    /// <summary>
    /// Reveals the first <paramref name="count"/> characters of the body text without
    /// re-laying-out the line (so the auto-sized font stays stable during the reveal).
    /// </summary>
    public void SetVisibleCharacters(int count)
    {
        if (bodyText == null) return;
        bodyText.maxVisibleCharacters = Mathf.Max(0, count);
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
