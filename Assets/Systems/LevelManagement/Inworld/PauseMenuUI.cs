using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// In-level pause / settings menu.
//
// Press ESC (or call Toggle()) to open/close. While open, the game is paused
// (Time.timeScale = 0) and the cursor is unlocked so players can click buttons.
//
// Buttons
//   RESUME         — close menu and resume play
//   RESET LEVEL    — reload the current level; fuel already collected is kept
//                    because FuelSessionData lives on the DontDestroyOnLoad relay
//   RETURN TO HUB  — unload the level and go back to the Space Hub
//
// Fuel display reads from FuelSessionData.Instance (authoritative) with a
// channel-based fallback, mirroring the approach used in FuelTerminalUI.
//
// UI is built entirely at runtime from the Sci-Fi UI sprite atlas so no
// prefab or extra scene assets are required — just drop the script on a
// Canvas and assign the four inspector references.
//
// Added by: Katie Trinh

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class PauseMenuUI : MonoBehaviour
{
    // ---- Inspector ----
    [Header("Fuel Data")]
    [SerializeField] private FuelStateChannel fuelStateChannel;
    [SerializeField] private int fallbackFuelTarget = 8;

    [Header("Sci-Fi UI Assets (atlas.png from Sci-Fi UI package)")]
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite buttonHighlightedSprite;
    [SerializeField] private Sprite buttonPressedSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("HUD Button (pause.png from Sci-Fi UI package)")]
    [SerializeField] private Sprite pauseButtonSprite;

    [Header("Scene Navigation")]
    [Tooltip("Fallback hub scene name used when LevelManager is absent (direct editor play).")]
    [SerializeField] private string fallbackHubScene = "J'sSpaceHub";

    [Header("Runtime References (populated automatically)")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI fuelLabel;
    [SerializeField] private GameObject hudButtonRoot;

    // ---- Public state ----
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    // ----------------------------------------------------------------
    // Unity lifecycle
    // ----------------------------------------------------------------

    private void Awake()
    {
        ConfigureCanvas();
        BuildRuntimeUIIfNeeded();
        SetOpen(false);          // hidden by default
    }

    private void OnEnable()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.OnRaised += HandleFuelState;

        RefreshFuel();
    }

    private void OnDisable()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.OnRaised -= HandleFuelState;

        // Safety: always restore time-scale when the component is disabled
        // (e.g. scene unload mid-pause).
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    // ----------------------------------------------------------------
    // Public API
    // ----------------------------------------------------------------

    public void Toggle() { UIClickSound.Play(); SetOpen(!IsOpen); }

    /// <summary>Close the menu and resume gameplay.</summary>
    public void Resume() { UIClickSound.Play(); SetOpen(false); }

    /// <summary>
    /// Reload the current level from its start. Fuel already collected is
    /// preserved — FuelSessionData persists via the DontDestroyOnLoad relay.
    /// </summary>
    public void ResetLevel()
    {
        UIClickSound.Play();
        SetOpen(false);
        Time.timeScale = 1f;

        if (LevelManager.Instance != null && LevelManager.Instance.CurrentWorld != null)
        {
            // Route through LevelManager so the loading screen plays and the
            // manager stays in sync. keepCheckpoint = false restarts from spawn.
            LevelManager.Instance.LoadWorld(LevelManager.Instance.CurrentWorld, false);
        }
        else
        {
            // Fallback for direct editor play-in-scene without LevelManager.
            LoadingScreenManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Return to the Space Hub. Fuel already collected is preserved.
    /// </summary>
    public void ReturnToHub()
    {
        UIClickSound.Play();
        SetOpen(false);
        Time.timeScale = 1f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadHub();
        }
        else
        {
            LoadingScreenManager.LoadScene(fallbackHubScene);
        }
    }

    // ----------------------------------------------------------------
    // Private helpers
    // ----------------------------------------------------------------

    private void SetOpen(bool open)
    {
        BuildRuntimeUIIfNeeded();
        if (panelRoot == null) return;
        if (panelRoot.activeSelf == open) return;

        panelRoot.SetActive(open);
        Time.timeScale = open ? 0f : 1f;

        if (open) UIInputBlocker.Push(); else UIInputBlocker.Pop();

        // HUD button hides while the panel is open so it doesn't overlap.
        if (hudButtonRoot != null)
            hudButtonRoot.SetActive(!open);

        if (open)
        {
            RefreshFuel();
            CursorHelper.Unlock();
        }
        else
        {
            CursorHelper.Lock();
        }
    }

    private void HandleFuelState(FuelState state)
    {
        SetFuelReadout(state.collected, state.target);
    }

    private void RefreshFuel()
    {
        // Prefer the authoritative persistent store.
        if (FuelSessionData.Instance != null)
        {
            SetFuelReadout(FuelSessionData.Instance.Collected, FuelSessionData.Instance.Target);
            return;
        }

        // Fall back to the channel's last broadcast for late subscribers.
        if (fuelStateChannel != null && fuelStateChannel.HasValue)
        {
            FuelState s = fuelStateChannel.LastValue;
            SetFuelReadout(s.collected, s.target);
            return;
        }

        SetFuelReadout(0, fallbackFuelTarget);
    }

    private void SetFuelReadout(int collected, int target)
    {
        if (fuelLabel == null) return;
        int safeC = Mathf.Max(0, collected);
        int safeT = Mathf.Max(1, target);
        fuelLabel.text = $"FUEL COLLECTED:  {safeC} / {safeT}";
    }

    // ----------------------------------------------------------------
    // Canvas configuration
    // ----------------------------------------------------------------

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;   // above FuelTerminalUI (50)
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    // ----------------------------------------------------------------
    // Procedural UI construction
    // ----------------------------------------------------------------

    private void BuildRuntimeUIIfNeeded()
    {
        if (panelRoot != null && fuelLabel != null)
            return;

        // ---- HUD open-menu button (always visible, top-right corner) ----
        hudButtonRoot = CreateUIObject("HUD Pause Button", transform);
        var hudRect = hudButtonRoot.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(1f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot     = new Vector2(1f, 1f);
        hudRect.anchoredPosition = new Vector2(-20f, -20f);
        hudRect.sizeDelta = new Vector2(72f, 72f);

        // Background image (optional sci-fi button sprite; falls back to plain)
        var hudImg = hudButtonRoot.AddComponent<Image>();
        hudImg.sprite         = buttonSprite;
        hudImg.type           = Image.Type.Sliced;
        hudImg.preserveAspect = false;
        hudImg.color          = new Color(0.15f, 0.55f, 0.6f, 0.85f);

        var hudBtn = hudButtonRoot.AddComponent<Button>();
        hudBtn.targetGraphic = hudImg;
        hudBtn.transition    = Selectable.Transition.ColorTint;
        hudBtn.colors        = new ColorBlock
        {
            normalColor      = new Color(0.15f, 0.55f, 0.6f, 0.85f),
            highlightedColor = new Color(0.3f, 0.85f, 0.9f, 1f),
            pressedColor     = new Color(0.05f, 0.35f, 0.4f, 1f),
            selectedColor    = new Color(0.15f, 0.55f, 0.6f, 0.85f),
            disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.5f),
            fadeDuration     = 0.1f,
            colorMultiplier  = 1f,
        };
        hudBtn.onClick.AddListener(Toggle);

        // ⚙ gear glyph overlaid on the button
        var gearLabel = CreateText("Gear Icon", hudButtonRoot.transform, "\u2699", 38, FontStyles.Bold);
        SetRect(gearLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        gearLabel.color = new Color(0.68f, 1f, 1f, 1f);
        gearLabel.raycastTarget = false;

        // ---- Background panel ----
        panelRoot = CreateUIObject("Pause Panel", transform);
        Image panelImg = panelRoot.AddComponent<Image>();
        panelImg.sprite = panelSprite;
        panelImg.type = Image.Type.Sliced;
        panelImg.preserveAspect = false;
        panelImg.color = new Color(0.7f, 1f, 1f, 0.95f);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot    = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(640f, 520f);

        // ---- Title ----
        var title = CreateText("Title", panelRoot.transform, "MISSION CONTROL", 40, FontStyles.Bold);
        SetRect(title.rectTransform,
            new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f),
            Vector2.zero, Vector2.zero);
        title.color = new Color(0.68f, 1f, 1f, 1f);

        // ---- Fuel label ----
        fuelLabel = CreateText("Fuel Label", panelRoot.transform,
            $"FUEL COLLECTED:  0 / {fallbackFuelTarget}", 26, FontStyles.Normal);
        SetRect(fuelLabel.rectTransform,
            new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.82f),
            Vector2.zero, Vector2.zero);
        fuelLabel.color = new Color(0.44f, 0.95f, 1f, 1f);

        // ---- Divider hint ----
        var hint = CreateText("Hint", panelRoot.transform,
            "Progress is auto-saved  \u2022  Press ESC or \u2699 to toggle", 18, FontStyles.Normal);
        SetRect(hint.rectTransform,
            new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.70f),
            Vector2.zero, Vector2.zero);
        hint.color = new Color(0.6f, 0.9f, 0.9f, 0.7f);

        // ---- RESUME button ----
        var resumeBtn = CreateButton("Resume Button", panelRoot.transform, "RESUME");
        SetRect(resumeBtn.GetComponent<RectTransform>(),
            new Vector2(0.18f, 0.48f), new Vector2(0.82f, 0.61f),
            Vector2.zero, Vector2.zero);
        resumeBtn.onClick.AddListener(Resume);

        // ---- RESET LEVEL button ----
        var resetBtn = CreateButton("Reset Level Button", panelRoot.transform, "RESET LEVEL");
        SetRect(resetBtn.GetComponent<RectTransform>(),
            new Vector2(0.18f, 0.30f), new Vector2(0.82f, 0.43f),
            Vector2.zero, Vector2.zero);
        resetBtn.onClick.AddListener(ResetLevel);

        // ---- RETURN TO HUB button ----
        var hubBtn = CreateButton("Return to Hub Button", panelRoot.transform, "RETURN TO HUB");
        SetRect(hubBtn.GetComponent<RectTransform>(),
            new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.25f),
            Vector2.zero, Vector2.zero);
        hubBtn.onClick.AddListener(ReturnToHub);
    }

    // ----------------------------------------------------------------
    // UI factory helpers (mirrors FuelTerminalUI conventions)
    // ----------------------------------------------------------------

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
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
        img.sprite           = buttonSprite;
        img.type             = Image.Type.Sliced;
        img.preserveAspect   = false;
        img.color            = Color.white;

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

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin      = anchorMin;
        rect.anchorMax      = anchorMax;
        rect.offsetMin      = offsetMin;
        rect.offsetMax      = offsetMax;
        rect.pivot          = new Vector2(0.5f, 0.5f);
        rect.localScale     = Vector3.one;
        rect.localRotation  = Quaternion.identity;
    }
}
