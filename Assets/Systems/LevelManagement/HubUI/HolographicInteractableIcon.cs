using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HolographicInteractableIcon : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private string keyLabel = "E";
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private float canvasScale = 0.00275f;
    [SerializeField] private Vector2 iconSize = new Vector2(96f, 96f);

    [Header("Style")]
    [SerializeField] private Color idleColor = new Color(0.23f, 0.95f, 1f, 0.78f);
    [SerializeField] private Color highlightedColor = new Color(0.72f, 1f, 1f, 1f);
    [SerializeField] private Color fillColor = new Color(0.04f, 0.38f, 0.46f, 0.28f);
    [SerializeField] private Color textColor = new Color(0.82f, 1f, 1f, 1f);

    [Header("Motion")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 2.2f;
    [SerializeField] private float pulseAmount = 0.08f;
    [SerializeField] private float pulseSpeed = 3.1f;
    [SerializeField] private bool faceCamera = true;

    private RectTransform iconRoot;
    private HolographicHexagonGraphic glowHexagon;
    private HolographicHexagonGraphic fillHexagon;
    private HolographicHexagonGraphic rimHexagon;
    private TextMeshProUGUI keyText;
    private bool visible = true;
    private bool highlighted;

    private void Awake()
    {
        BuildIconIfNeeded();
        RefreshVisuals();
    }

    private void OnEnable()
    {
        BuildIconIfNeeded();
        RefreshVisuals();
    }

    private void LateUpdate()
    {
        if (iconRoot == null)
            return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        iconRoot.localPosition = localOffset + Vector3.up * bob;
        iconRoot.localScale = Vector3.one * canvasScale * pulse;

        if (!faceCamera)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 toCamera = iconRoot.position - camera.transform.position;
        if (toCamera.sqrMagnitude <= 0.0001f)
            return;

        iconRoot.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    public void SetVisible(bool isVisible)
    {
        visible = isVisible;
        RefreshVisuals();
    }

    public void SetHighlighted(bool isHighlighted)
    {
        highlighted = isHighlighted;
        RefreshVisuals();
    }

    private void BuildIconIfNeeded()
    {
        if (iconRoot != null)
            return;

        GameObject rootObject = new GameObject("Holographic Interact Icon", typeof(RectTransform), typeof(Canvas));
        rootObject.layer = gameObject.layer;
        rootObject.transform.SetParent(transform, false);

        iconRoot = rootObject.GetComponent<RectTransform>();
        iconRoot.anchorMin = new Vector2(0.5f, 0.5f);
        iconRoot.anchorMax = new Vector2(0.5f, 0.5f);
        iconRoot.pivot = new Vector2(0.5f, 0.5f);
        iconRoot.sizeDelta = iconSize;
        iconRoot.localPosition = localOffset;
        iconRoot.localScale = Vector3.one * canvasScale;

        Canvas canvas = rootObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 20;

        glowHexagon = CreateHexagon("Glow", iconRoot, iconSize * 1.18f, false, 0f);
        fillHexagon = CreateHexagon("Fill", iconRoot, iconSize, false, 0f);
        rimHexagon = CreateHexagon("Rim", iconRoot, iconSize, true, 8f);

        GameObject textObject = CreateUIObject("Key Label", iconRoot);
        keyText = textObject.AddComponent<TextMeshProUGUI>();
        keyText.text = keyLabel;
        keyText.alignment = TextAlignmentOptions.Center;
        keyText.fontSize = 46f;
        keyText.fontStyle = FontStyles.Bold;
        keyText.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private HolographicHexagonGraphic CreateHexagon(string objectName, Transform parent, Vector2 size, bool outlineOnly, float outlineThickness)
    {
        GameObject hexObject = CreateUIObject(objectName, parent);
        HolographicHexagonGraphic hexagon = hexObject.AddComponent<HolographicHexagonGraphic>();
        hexagon.OutlineOnly = outlineOnly;
        hexagon.OutlineThickness = outlineThickness;
        hexagon.raycastTarget = false;

        RectTransform rect = hexObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        return hexagon;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        uiObject.layer = gameObject.layer;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private void RefreshVisuals()
    {
        if (iconRoot == null)
            return;

        iconRoot.gameObject.SetActive(visible);

        Color accent = highlighted ? highlightedColor : idleColor;
        if (glowHexagon != null)
            glowHexagon.color = new Color(accent.r, accent.g, accent.b, highlighted ? 0.24f : 0.12f);

        if (fillHexagon != null)
            fillHexagon.color = highlighted
                ? new Color(fillColor.r, fillColor.g, fillColor.b, Mathf.Min(fillColor.a + 0.14f, 0.7f))
                : fillColor;

        if (rimHexagon != null)
            rimHexagon.color = accent;

        if (keyText != null)
        {
            keyText.text = keyLabel;
            keyText.color = highlighted ? highlightedColor : textColor;
        }
    }
}
