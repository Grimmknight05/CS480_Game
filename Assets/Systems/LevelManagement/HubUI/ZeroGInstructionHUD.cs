using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZeroGInstructionHUD : MonoBehaviour
{
    private static ZeroGInstructionHUD instance;
    private static int activeRequests;

    [Header("Sci-Fi UI Assets")]
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite accentSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Runtime References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI bodyLabel;
    [SerializeField] private TextMeshProUGUI rightColumnLabel;
    private bool isVisible;

    private void Awake()
    {
        instance = this;
        activeRequests = 0;
        ConfigureCanvas();
        BuildRuntimeUIIfNeeded();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Show()
    {
        ZeroGInstructionHUD hud = GetInstance();
        if (hud == null)
            return;

        activeRequests++;
        hud.SetVisible(true);
    }

    public static void Hide()
    {
        ZeroGInstructionHUD hud = GetInstance();
        if (hud == null)
            return;

        activeRequests = Mathf.Max(0, activeRequests - 1);
        hud.SetVisible(activeRequests > 0);
    }

    private static ZeroGInstructionHUD GetInstance()
    {
        if (instance != null)
            return instance;

        instance = FindFirstObjectByType<ZeroGInstructionHUD>(FindObjectsInactive.Include);
        return instance;
    }

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 55);
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
        if (panelRoot != null && bodyLabel != null && rightColumnLabel != null)
            return;

        panelRoot = CreateUIObject("Zero-G Instructions Panel", transform);
        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.sprite = panelSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.preserveAspect = false;
        panelImage.raycastTarget = false;
        panelImage.color = new Color(0.62f, 1f, 1f, 0.92f);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-28f, -28f);
        panelRect.sizeDelta = new Vector2(420f, 172f);

        GameObject accent = CreateUIObject("Accent Bar", panelRoot.transform);
        Image accentImage = accent.AddComponent<Image>();
        accentImage.sprite = accentSprite;
        accentImage.type = Image.Type.Sliced;
        accentImage.raycastTarget = false;
        accentImage.color = new Color(0.1f, 0.95f, 1f, 0.95f);
        SetRect(accent.GetComponent<RectTransform>(), new Vector2(0.12f, 0.78f), new Vector2(0.88f, 0.83f), Vector2.zero, Vector2.zero);

        TextMeshProUGUI title = CreateText("Title", panelRoot.transform, "ZERO-G NAV", 23, FontStyles.Bold);
        title.color = new Color(0.62f, 1f, 1f, 1f);
        SetRect(title.rectTransform, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.78f), Vector2.zero, Vector2.zero);

        bodyLabel = CreateText("Left Column", panelRoot.transform, "\u2022 WASD move\n\u2022 Space rise", 18, FontStyles.Normal);
        bodyLabel.color = Color.white;
        bodyLabel.alignment = TextAlignmentOptions.TopLeft;
        bodyLabel.lineSpacing = 8f;
        SetRect(bodyLabel.rectTransform, new Vector2(0.12f, 0.18f), new Vector2(0.5f, 0.55f), Vector2.zero, Vector2.zero);

        rightColumnLabel = CreateText("Right Column", panelRoot.transform, "\u2022 Mouse aim\n\u2022 C descend", 18, FontStyles.Normal);
        rightColumnLabel.color = Color.white;
        rightColumnLabel.alignment = TextAlignmentOptions.TopLeft;
        rightColumnLabel.lineSpacing = 8f;
        SetRect(rightColumnLabel.rectTransform, new Vector2(0.54f, 0.18f), new Vector2(0.88f, 0.55f), Vector2.zero, Vector2.zero);
    }

    private void SetVisible(bool visible)
    {
        BuildRuntimeUIIfNeeded();

        if (panelRoot == null)
            return;

        if (panelRoot.activeSelf != visible)
            panelRoot.SetActive(visible);

        isVisible = visible;
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
}
