using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FuelTerminalUI : MonoBehaviour
{
    [Header("Fuel Data")]
    [SerializeField] private FuelStateChannel fuelStateChannel;
    [SerializeField] private int fallbackFuelTarget = 4;

    [Header("Sci-Fi UI Assets")]
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite buttonHighlightedSprite;
    [SerializeField] private Sprite buttonPressedSprite;
    [SerializeField] private Sprite progressBackgroundSprite;
    [SerializeField] private Sprite fuelFillSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Runtime References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI fuelAmountLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private Image fuelFillImage;

    public event Action<bool> VisibilityChanged;

    private GameManager subscribedGameManager;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        ConfigureCanvas();
        BuildRuntimeUIIfNeeded();
        SetOpen(false, false);
    }

    private void OnEnable()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.OnRaised += HandleFuelStateRaised;

        TrySubscribeToGameManager();
        RefreshFuelReadout();
    }

    private void Start()
    {
        TrySubscribeToGameManager();
        RefreshFuelReadout();
    }

    private void Update()
    {
        if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Hide();
    }

    private void OnDisable()
    {
        if (fuelStateChannel != null)
            fuelStateChannel.OnRaised -= HandleFuelStateRaised;

        UnsubscribeFromGameManager();
    }

    public void Toggle()
    {
        SetOpen(!IsOpen, true);
    }

    public void Show()
    {
        SetOpen(true, true);
    }

    public void Hide()
    {
        SetOpen(false, true);
    }

    private void SetOpen(bool open, bool adjustCursor)
    {
        BuildRuntimeUIIfNeeded();

        if (panelRoot == null)
            return;

        if (panelRoot.activeSelf == open)
            return;

        panelRoot.SetActive(open);

        if (open)
        {
            RefreshFuelReadout();
            if (adjustCursor) CursorHelper.Unlock();
        }
        else if (adjustCursor)
        {
            CursorHelper.Lock();
        }

        VisibilityChanged?.Invoke(open);
    }

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private void BuildRuntimeUIIfNeeded()
    {
        if (panelRoot != null && fuelAmountLabel != null && statusLabel != null && fuelFillImage != null)
            return;

        panelRoot = CreateUIObject("Fuel Terminal Panel", transform);
        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.sprite = panelSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.preserveAspect = false;
        panelImage.color = new Color(0.7f, 1f, 1f, 0.95f);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 440f);

        TextMeshProUGUI title = CreateText("Title", panelRoot.transform, "FUEL RESERVES", 38, FontStyles.Bold);
        SetRect(title.rectTransform, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);
        title.color = new Color(0.68f, 1f, 1f, 1f);

        fuelAmountLabel = CreateText("Fuel Amount", panelRoot.transform, "0 / 4", 80, FontStyles.Bold);
        SetRect(fuelAmountLabel.rectTransform, new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.7f), Vector2.zero, Vector2.zero);
        fuelAmountLabel.color = Color.white;

        statusLabel = CreateText("Status", panelRoot.transform, "COLLECT FUEL FROM PLANETS", 26, FontStyles.Normal);
        SetRect(statusLabel.rectTransform, new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.44f), Vector2.zero, Vector2.zero);
        statusLabel.color = new Color(0.44f, 0.95f, 1f, 1f);

        GameObject barBack = CreateUIObject("Fuel Bar Background", panelRoot.transform);
        Image barBackImage = barBack.AddComponent<Image>();
        barBackImage.sprite = progressBackgroundSprite;
        barBackImage.type = Image.Type.Sliced;
        barBackImage.color = new Color(0.08f, 0.35f, 0.42f, 0.9f);
        SetRect(barBack.GetComponent<RectTransform>(), new Vector2(0.16f, 0.2f), new Vector2(0.84f, 0.28f), Vector2.zero, Vector2.zero);

        GameObject fill = CreateUIObject("Fuel Bar Fill", barBack.transform);
        fuelFillImage = fill.AddComponent<Image>();
        fuelFillImage.sprite = fuelFillSprite;
        fuelFillImage.type = Image.Type.Filled;
        fuelFillImage.fillMethod = Image.FillMethod.Horizontal;
        fuelFillImage.fillOrigin = 0;
        fuelFillImage.color = new Color(0.16f, 1f, 0.55f, 1f);
        SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(14f, 8f), new Vector2(-14f, -8f));

        Button closeButton = CreateButton("Close Button", panelRoot.transform, "CLOSE");
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.36f, 0.06f), new Vector2(0.64f, 0.17f), Vector2.zero, Vector2.zero);
        closeButton.onClick.AddListener(Hide);
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        uiObject.layer = gameObject.layer;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, string text, float size, FontStyles style)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.raycastTarget = false;

        if (fontAsset != null)
            label.font = fontAsset;

        return label;
    }

    private Button CreateButton(string objectName, Transform parent, string labelText)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState
        {
            highlightedSprite = buttonHighlightedSprite,
            selectedSprite = buttonHighlightedSprite,
            pressedSprite = buttonPressedSprite
        };

        TextMeshProUGUI label = CreateText("Label", buttonObject.transform, labelText, 24, FontStyles.Bold);
        label.color = Color.white;
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return button;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void TrySubscribeToGameManager()
    {
        if (subscribedGameManager == GameManager.Instance)
            return;

        UnsubscribeFromGameManager();

        if (GameManager.Instance == null)
            return;

        subscribedGameManager = GameManager.Instance;
        subscribedGameManager.FuelChanged += HandleFuelChanged;
    }

    private void UnsubscribeFromGameManager()
    {
        if (subscribedGameManager == null)
            return;

        subscribedGameManager.FuelChanged -= HandleFuelChanged;
        subscribedGameManager = null;
    }

    private void HandleFuelChanged(int collected, int target)
    {
        SetFuelReadout(collected, target);
    }

    private void HandleFuelStateRaised(FuelState state)
    {
        SetFuelReadout(state.collected, state.target);
    }

    private void RefreshFuelReadout()
    {
        TrySubscribeToGameManager();

        if (GameManager.Instance != null)
        {
            SetFuelReadout(GameManager.Instance.FuelCollected, GameManager.Instance.FuelTarget);
            return;
        }

        if (fuelStateChannel != null && fuelStateChannel.HasValue)
        {
            FuelState state = fuelStateChannel.LastValue;
            SetFuelReadout(state.collected, state.target);
            return;
        }

        SetFuelReadout(0, fallbackFuelTarget);
    }

    private void SetFuelReadout(int collected, int target)
    {
        int safeCollected = Mathf.Max(0, collected);
        int safeTarget = Mathf.Max(0, target);
        int denominator = Mathf.Max(1, safeTarget);

        if (fuelAmountLabel != null)
            fuelAmountLabel.text = $"{safeCollected} / {safeTarget}";

        if (statusLabel != null)
            statusLabel.text = safeCollected >= safeTarget && safeTarget > 0
                ? "SHIP FUEL READY"
                : "COLLECT FUEL FROM PLANETS";

        if (fuelFillImage != null)
            fuelFillImage.fillAmount = Mathf.Clamp01((float)safeCollected / denominator);
    }
}
