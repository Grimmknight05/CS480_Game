using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Title-screen main menu for Gravity Guy.
//
// Panels
//   Main    — GRAVITY GUY title, Play / Settings / Credits buttons
//   Settings — Master Volume slider, Mouse Sensitivity slider,
//              Graphics quality selector, Controls reference
//   Credits  — Team names and course info
//
// All UI is built procedurally from the Sci-Fi UI sprite atlas so no
// prefab or extra scene assets are required beyond the four inspector
// sprite references.
//
// PlayerPrefs keys (shared with OrbitalCamera):
//   MasterVolume     float 0–1
//   MouseSensitivity float 0.05–0.8
//
// Added by: Katie Trinh

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class MainMenuUI : MonoBehaviour
{
    // ---- Inspector ----
    [Header("Sci-Fi UI Assets (atlas.png)")]
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite buttonHighlightedSprite;
    [SerializeField] private Sprite buttonPressedSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Background (Space.tga.jpg from Sci-Fi UI package)")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("Scene Names")]
    [SerializeField] private string hubSceneName      = "J'sSpaceHub";
    [SerializeField] private string loadingSceneName  = "LoadingScreen";

    // PlayerPrefs keys
    private const string PrefMasterVol   = "MasterVolume";
    private const string PrefSensitivity = "MouseSensitivity";

    // Default values
    private const float DefaultMasterVol   = 1f;
    private const float DefaultSensitivity = 0.2f;

    // Runtime panel roots
    private GameObject mainPanel;
    private GameObject settingsPanel;
    private GameObject creditsPanel;

    // ----------------------------------------------------------------
    // Unity lifecycle
    // ----------------------------------------------------------------

    private void Awake()
    {
        UIInputBlocker.Reset();
        ConfigureCanvas();
        BuildUI();
        ShowMain();
        CursorHelper.Unlock();
        ApplySavedSettings();
    }

    // ----------------------------------------------------------------
    // Panel navigation
    // ----------------------------------------------------------------

    private void ShowMain()
    {
        SetPanels(main: true, settings: false, credits: false);
    }

    private void BackToMain()
    {
        UIClickSound.Play();
        ShowMain();
    }

    private void ShowSettings()
    {
        UIClickSound.Play();
        SetPanels(main: false, settings: true, credits: false);
    }

    private void ShowCredits()
    {
        UIClickSound.Play();
        SetPanels(main: false, settings: false, credits: true);
    }

    private void SetPanels(bool main, bool settings, bool credits)
    {
        if (mainPanel     != null) mainPanel.SetActive(main);
        if (settingsPanel != null) settingsPanel.SetActive(settings);
        if (creditsPanel  != null) creditsPanel.SetActive(credits);
    }

    // ----------------------------------------------------------------
    // Actions
    // ----------------------------------------------------------------

    private void Play()
    {
        UIClickSound.Play();
        LoadingScreenManager.LoadScene(hubSceneName);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ----------------------------------------------------------------
    // Settings persistence
    // ----------------------------------------------------------------

    private void ApplySavedSettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(PrefMasterVol, DefaultMasterVol);
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PrefMasterVol, value);
        PlayerPrefs.Save();
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(PrefSensitivity, value);
        PlayerPrefs.Save();
    }

    private void SetQuality(int level)
    {
        UIClickSound.Play();
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.SetInt("GraphicsQuality", level);
        PlayerPrefs.Save();
    }

    // ----------------------------------------------------------------
    // Canvas configuration
    // ----------------------------------------------------------------

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
        }
    }

    // ----------------------------------------------------------------
    // UI construction entry point
    // ----------------------------------------------------------------

    private void BuildUI()
    {
        BuildMainPanel();
        BuildSettingsPanel();
        BuildCreditsPanel();
    }

    // ---- Main panel ----
    private void BuildMainPanel()
    {
        mainPanel = CreateUIObject("Main Panel", transform);
        StretchFull(mainPanel.GetComponent<RectTransform>());

        // ---- Layer 1: Space background ----
        var bgImg = mainPanel.AddComponent<Image>();
        bgImg.sprite = backgroundSprite;
        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = false;
        bgImg.color = Color.white;

        // ---- Layer 2: Dark overlay so text stays readable ----
        var overlay = CreateUIObject("Overlay", mainPanel.transform);
        overlay.AddComponent<Image>().color = new Color(0.03f, 0.05f, 0.10f, 0.72f);
        StretchFull(overlay.GetComponent<RectTransform>());

        // ---- Layer 3: Title glow band ----
        var titleGlow = CreateUIObject("Title Glow", mainPanel.transform);
        titleGlow.AddComponent<Image>().color = new Color(0.04f, 0.30f, 0.42f, 0.50f);
        SetRect(titleGlow.GetComponent<RectTransform>(),
            new Vector2(0f, 0.58f), new Vector2(1f, 0.97f),
            Vector2.zero, Vector2.zero);

        // Bright top edge line
        var topLine = CreateUIObject("Top Line", mainPanel.transform);
        topLine.AddComponent<Image>().color = new Color(0.44f, 0.95f, 1f, 0.90f);
        SetRect(topLine.GetComponent<RectTransform>(),
            new Vector2(0f, 0.965f), new Vector2(1f, 0.972f),
            Vector2.zero, Vector2.zero);

        // Dim bottom edge line below title band
        var bandBottomLine = CreateUIObject("Band Bottom Line", mainPanel.transform);
        bandBottomLine.AddComponent<Image>().color = new Color(0.3f, 0.85f, 0.95f, 0.50f);
        SetRect(bandBottomLine.GetComponent<RectTransform>(),
            new Vector2(0f, 0.580f), new Vector2(1f, 0.584f),
            Vector2.zero, Vector2.zero);

        // ---- Title ----
        var title = CreateText("Title", mainPanel.transform, "GRAVITY GUY", 100, FontStyles.Bold);
        SetRect(title.rectTransform,
            new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.94f),
            Vector2.zero, Vector2.zero);
        title.color = new Color(0.68f, 1f, 1f, 1f);
        title.characterSpacing = 6f;

        // Sub-title  (spaced-out caps for a sci-fi look)
        var sub = CreateText("Sub", mainPanel.transform,
            "A  S P A C E  E X P L O R A T I O N  A D V E N T U R E", 20, FontStyles.Normal);
        SetRect(sub.rectTransform,
            new Vector2(0.1f, 0.595f), new Vector2(0.9f, 0.64f),
            Vector2.zero, Vector2.zero);
        sub.color = new Color(0.44f, 0.88f, 0.95f, 0.80f);

        // ---- Layer 4: Sci-fi card behind the buttons ----
        var btnCard = CreateUIObject("Button Card", mainPanel.transform);
        var btnCardImg = btnCard.AddComponent<Image>();
        btnCardImg.sprite = panelSprite;
        btnCardImg.type = Image.Type.Sliced;
        btnCardImg.color = new Color(0.08f, 0.28f, 0.38f, 0.65f);
        SetRect(btnCard.GetComponent<RectTransform>(),
            new Vector2(0.28f, 0.08f), new Vector2(0.72f, 0.57f),
            Vector2.zero, Vector2.zero);

        // Thin accent line above card
        var cardTopLine = CreateUIObject("Card Top Line", mainPanel.transform);
        cardTopLine.AddComponent<Image>().color = new Color(0.44f, 0.95f, 1f, 0.40f);
        SetRect(cardTopLine.GetComponent<RectTransform>(),
            new Vector2(0.28f, 0.567f), new Vector2(0.72f, 0.571f),
            Vector2.zero, Vector2.zero);

        // ---- Buttons ----
        var play = CreateButton("Play", mainPanel.transform, "PLAY");
        SetRect(play.GetComponent<RectTransform>(),
            new Vector2(0.35f, 0.43f), new Vector2(0.65f, 0.55f),
            Vector2.zero, Vector2.zero);
        play.onClick.AddListener(Play);

        var settingsBtn = CreateButton("Settings", mainPanel.transform, "SETTINGS");
        SetRect(settingsBtn.GetComponent<RectTransform>(),
            new Vector2(0.35f, 0.28f), new Vector2(0.65f, 0.40f),
            Vector2.zero, Vector2.zero);
        settingsBtn.onClick.AddListener(ShowSettings);

        var creditsBtn = CreateButton("Credits", mainPanel.transform, "CREDITS");
        SetRect(creditsBtn.GetComponent<RectTransform>(),
            new Vector2(0.35f, 0.13f), new Vector2(0.65f, 0.25f),
            Vector2.zero, Vector2.zero);
        creditsBtn.onClick.AddListener(ShowCredits);

#if !UNITY_WEBGL
        var quitBtn = CreateButton("Quit", mainPanel.transform, "QUIT");
        SetRect(quitBtn.GetComponent<RectTransform>(),
            new Vector2(0.38f, 0.09f), new Vector2(0.62f, 0.12f),
            Vector2.zero, Vector2.zero);
        // Smaller, less prominent quit
        var qLbl = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (qLbl != null) { qLbl.fontSize = 18; qLbl.color = new Color(0.7f, 0.7f, 0.7f, 0.8f); }
        quitBtn.onClick.AddListener(Quit);
#endif
    }

    // ---- Settings panel ----
    private void BuildSettingsPanel()
    {
        settingsPanel = CreateCentredPanel("Settings Panel", 720f, 620f);

        var title = CreateText("Title", settingsPanel.transform, "SETTINGS", 40, FontStyles.Bold);
        SetRect(title.rectTransform,
            new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f),
            Vector2.zero, Vector2.zero);
        title.color = new Color(0.68f, 1f, 1f, 1f);

        // --- Audio section ---
        AddSectionHeader(settingsPanel.transform, "AUDIO", 0.77f, 0.85f);

        AddSliderRow(settingsPanel.transform,
            label: "Master Volume",
            prefKey: PrefMasterVol,
            defaultVal: DefaultMasterVol,
            min: 0f, max: 1f,
            top: 0.76f, bottom: 0.66f,
            onChange: OnMasterVolumeChanged);

        // --- Camera section ---
        AddSectionHeader(settingsPanel.transform, "CAMERA", 0.60f, 0.66f);

        AddSliderRow(settingsPanel.transform,
            label: "Mouse Sensitivity",
            prefKey: PrefSensitivity,
            defaultVal: DefaultSensitivity,
            min: 0.05f, max: 0.8f,
            top: 0.59f, bottom: 0.49f,
            onChange: OnSensitivityChanged);

        // --- Graphics section ---
        AddSectionHeader(settingsPanel.transform, "GRAPHICS", 0.43f, 0.49f);
        AddQualityButtons(settingsPanel.transform, 0.32f, 0.42f);

        // --- Controls section ---
        AddSectionHeader(settingsPanel.transform, "CONTROLS", 0.24f, 0.31f);
        var controls = CreateText("ControlsInfo", settingsPanel.transform,
            "WASD — Move     Mouse — Look     Space — Jump / Rise     C — Descend (Zero-G)     Q — Switch Weapon     E — Interact     ESC / ⚙ — Pause",
            17, FontStyles.Normal);
        SetRect(controls.rectTransform,
            new Vector2(0.04f, 0.13f), new Vector2(0.96f, 0.24f),
            Vector2.zero, Vector2.zero);
        controls.color = new Color(0.5f, 0.85f, 0.9f, 0.85f);
        controls.textWrappingMode = TextWrappingModes.Normal;

        // Back button
        var back = CreateButton("Back", settingsPanel.transform, "BACK");
        SetRect(back.GetComponent<RectTransform>(),
            new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.12f),
            Vector2.zero, Vector2.zero);
        back.onClick.AddListener(BackToMain);
    }

    // ---- Credits panel ----
    private void BuildCreditsPanel()
    {
        creditsPanel = CreateCentredPanel("Credits Panel", 640f, 560f);

        var title = CreateText("Title", creditsPanel.transform, "CREDITS", 40, FontStyles.Bold);
        SetRect(title.rectTransform,
            new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f),
            Vector2.zero, Vector2.zero);
        title.color = new Color(0.68f, 1f, 1f, 1f);

        string body =
            "GRAVITY GUY\n\n" +
            "CS 480 — Design Patterns in Game Development\n" +
            "Spring 2026\n\n" +
            "Katie Trinh\n" +
            "Joshua Henrikson\n" +
            "David Haddad\n" +
            "Sarah Temple\n\n" +
            "© 2026";

        var credits = CreateText("Body", creditsPanel.transform, body, 22, FontStyles.Normal);
        SetRect(credits.rectTransform,
            new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.85f),
            Vector2.zero, Vector2.zero);
        credits.color = new Color(0.75f, 0.95f, 1f, 1f);
        credits.textWrappingMode = TextWrappingModes.Normal;
        credits.lineSpacing = 4f;

        var back = CreateButton("Back", creditsPanel.transform, "BACK");
        SetRect(back.GetComponent<RectTransform>(),
            new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.12f),
            Vector2.zero, Vector2.zero);
        back.onClick.AddListener(BackToMain);
    }

    // ----------------------------------------------------------------
    // Settings UI helpers
    // ----------------------------------------------------------------

    private void AddSectionHeader(Transform parent, string label, float top, float bottom)
    {
        var header = CreateText(label + " Header", parent, label, 18, FontStyles.Bold);
        SetRect(header.rectTransform,
            new Vector2(0.04f, bottom), new Vector2(0.96f, top),
            Vector2.zero, Vector2.zero);
        header.color = new Color(0.3f, 0.75f, 0.85f, 0.7f);
        header.alignment = TextAlignmentOptions.Left;
    }

    private void AddSliderRow(Transform parent, string label, string prefKey, float defaultVal,
        float min, float max, float top, float bottom, UnityEngine.Events.UnityAction<float> onChange)
    {
        // Label
        var lbl = CreateText(label + " Lbl", parent, label, 22, FontStyles.Normal);
        SetRect(lbl.rectTransform,
            new Vector2(0.04f, bottom), new Vector2(0.46f, top),
            Vector2.zero, Vector2.zero);
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.color = new Color(0.85f, 0.98f, 1f, 1f);

        // Slider
        float initialValue = PlayerPrefs.GetFloat(prefKey, defaultVal);
        var slider = CreateSlider(label + " Slider", parent, min, max, initialValue);
        SetRect(slider.GetComponent<RectTransform>(),
            new Vector2(0.47f, bottom + 0.01f), new Vector2(0.96f, top - 0.01f),
            Vector2.zero, Vector2.zero);
        slider.onValueChanged.AddListener(onChange);
    }

    private Image[] qualityButtonImages; // cached for dynamic highlight updates

    private void AddQualityButtons(Transform parent, float bottom, float top)
    {
        string[] labels = { "LOW", "MEDIUM", "HIGH" };
        float[] anchorsX  = { 0.04f, 0.37f, 0.70f };
        float[] anchorsX2 = { 0.33f, 0.66f, 0.96f };

        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        qualityButtonImages = new Image[3];

        for (int i = 0; i < 3; i++)
        {
            int level = i;
            var btn = CreateButton("Quality_" + labels[i], parent, labels[i]);
            SetRect(btn.GetComponent<RectTransform>(),
                new Vector2(anchorsX[i], bottom), new Vector2(anchorsX2[i], top),
                Vector2.zero, Vector2.zero);

            qualityButtonImages[i] = btn.GetComponent<Image>();
            btn.onClick.AddListener(() => { SetQuality(level); RefreshQualityHighlight(level); });
        }

        RefreshQualityHighlight(savedQuality);
    }

    private void RefreshQualityHighlight(int activeLevel)
    {
        if (qualityButtonImages == null) return;
        for (int i = 0; i < qualityButtonImages.Length; i++)
        {
            if (qualityButtonImages[i] == null) continue;
            qualityButtonImages[i].color = (i == activeLevel)
                ? new Color(0.2f, 0.7f, 0.8f, 1f)   // selected: teal
                : Color.white;                        // unselected: default
        }
    }

    // ----------------------------------------------------------------
    // Widget factories
    // ----------------------------------------------------------------

    private GameObject CreateCentredPanel(string name, float width, float height)
    {
        var go = CreateUIObject(name, transform);
        var img = go.AddComponent<Image>();
        img.sprite = panelSprite;
        img.type   = Image.Type.Sliced;
        img.color  = new Color(0.7f, 1f, 1f, 0.95f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(width, height);
        return go;
    }

    private Slider CreateSlider(string objName, Transform parent, float min, float max, float initialValue)
    {
        var root = CreateUIObject(objName, parent);
        var bgImg = root.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.35f, 0.42f, 0.9f);

        // Fill Area
        var fillArea = CreateUIObject("Fill Area", root.transform);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f);
        faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f,   0f);
        faRT.offsetMax = new Vector2(-15f, 0f);

        // Fill
        var fill = CreateUIObject("Fill", fillArea.transform);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.16f, 1f, 0.55f, 1f);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f); // .x set by Slider each frame
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = new Vector2(10f, 0f);

        // Handle Slide Area
        var handleArea = CreateUIObject("Handle Slide Area", root.transform);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = new Vector2(0f, 0f);
        haRT.anchorMax = new Vector2(1f, 1f);
        haRT.offsetMin = new Vector2(10f,  0f);
        haRT.offsetMax = new Vector2(-10f, 0f);

        // Handle
        var handle = CreateUIObject("Handle", handleArea.transform);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.68f, 1f, 1f, 1f);
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0f, 0.5f);
        handleRT.anchorMax = new Vector2(0f, 0.5f);
        handleRT.pivot     = new Vector2(0.5f, 0.5f);
        handleRT.sizeDelta = new Vector2(20f, 30f);

        // Slider
        var slider = root.AddComponent<Slider>();
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = min;
        slider.maxValue     = max;
        slider.wholeNumbers = false;
        slider.fillRect     = fillRT;
        slider.handleRect   = handleRT;
        slider.targetGraphic = handleImg;
        slider.value        = initialValue; // triggers UpdateVisuals
        return slider;
    }

    private GameObject CreateUIObject(string objName, Transform parent)
    {
        var go = new GameObject(objName, typeof(RectTransform), typeof(CanvasRenderer));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);
        return go;
    }

    private TextMeshProUGUI CreateText(
        string objName, Transform parent, string text, float size, FontStyles style)
    {
        var go    = CreateUIObject(objName, parent);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text             = text;
        label.fontSize         = size;
        label.fontStyle        = style;
        label.alignment        = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget    = false;
        if (fontAsset != null) label.font = fontAsset;
        return label;
    }

    private Button CreateButton(string objName, Transform parent, string labelText)
    {
        var go  = CreateUIObject(objName, parent);
        var img = go.AddComponent<Image>();
        img.sprite = buttonSprite;
        img.type   = Image.Type.Sliced;
        img.color  = Color.white;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition    = Selectable.Transition.SpriteSwap;
        btn.spriteState   = new SpriteState
        {
            highlightedSprite = buttonHighlightedSprite,
            selectedSprite    = buttonHighlightedSprite,
            pressedSprite     = buttonPressedSprite,
        };

        var lbl = CreateText("Label", go.transform, labelText, 24, FontStyles.Bold);
        lbl.color = Color.white;
        SetRect(lbl.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return btn;
    }

    // ----------------------------------------------------------------
    // RectTransform helpers
    // ----------------------------------------------------------------

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin     = anchorMin;
        rect.anchorMax     = anchorMax;
        rect.offsetMin     = offsetMin;
        rect.offsetMax     = offsetMax;
        rect.pivot         = new Vector2(0.5f, 0.5f);
        rect.localScale    = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
