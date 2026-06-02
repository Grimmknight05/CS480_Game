using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialScreenUI : MonoBehaviour
{
    [Header("Tutorial Content")]
    [SerializeField] private string title = "SHIP SYSTEMS TUTORIAL";
    [SerializeField] private string introText = "Welcome aboard the Space Hub.\nYour suit link is active.\nReview mission systems before launch.";
    [SerializeField] private string movementText = "WASD move\nMouse aim\nSpace jump or rise in zero-g\nC descend in zero-g";
    [SerializeField] private string interactText = "Look for cyan holograms\nPress E near terminals, buttons, and ship systems";
    [SerializeField] private string objectiveText = "Explore planets\nCollect fuel\nReturn to the Space Hub to launch";
    [SerializeField] private Sprite movementImage;
    [SerializeField] private Sprite interactImage;
    [SerializeField] private Sprite objectiveImage;

    [Header("Screenshot Images")]
    [SerializeField] private Texture2D movementScreenshot;
    [SerializeField] private Texture2D interactScreenshot;
    [SerializeField] private Texture2D objectiveScreenshot;

    [Header("Sci-Fi UI Assets")]
    [SerializeField] private Sprite panelSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private Sprite buttonHighlightedSprite;
    [SerializeField] private Sprite buttonPressedSprite;
    [SerializeField] private Sprite imageFrameSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Runtime References")]
    [SerializeField] private GameObject panelRoot;

    public event Action<bool> VisibilityChanged;

    private GameObject[] pageRoots;
    private Button backButton;
    private Button nextButton;
    private Button bottomCloseButton;
    private TextMeshProUGUI pageCounterLabel;
    private int currentPageIndex;
    private int openedFrame = -1;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        ConfigureCanvas();
        BuildRuntimeUIIfNeeded();
        SetOpen(false, false);
    }

    private void OnEnable()
    {
        InteractionInputBridge.OnInteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        InteractionInputBridge.OnInteractPressed -= HandleInteractPressed;
    }

    private void Update()
    {
        if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Hide();
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

    private void HandleInteractPressed()
    {
        if (Time.frameCount == openedFrame)
            return;

        if (!IsOpen || pageRoots == null || currentPageIndex >= pageRoots.Length - 1)
            return;

        ShowNextPage();
    }

    private void SetOpen(bool open, bool adjustCursor)
    {
        BuildRuntimeUIIfNeeded();

        if (panelRoot == null || panelRoot.activeSelf == open)
            return;

        if (open)
        {
            openedFrame = Time.frameCount;
            ShowPage(0);
        }

        panelRoot.SetActive(open);

        if (adjustCursor)
        {
            if (open)
                CursorHelper.Unlock();
            else
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
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 52);
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
        if (panelRoot != null && pageRoots != null)
            return;

        panelRoot = CreateUIObject("Tutorial Panel", transform);
        Image panelImage = panelRoot.AddComponent<Image>();
        panelImage.sprite = panelSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.preserveAspect = false;
        panelImage.color = new Color(0.66f, 1f, 1f, 0.96f);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(1420f, 840f);

        TextMeshProUGUI heading = CreateText("Title", panelRoot.transform, title, 44f, FontStyles.Bold);
        heading.color = new Color(0.68f, 1f, 1f, 1f);
        SetRect(heading.rectTransform, new Vector2(0.08f, 0.87f), new Vector2(0.86f, 0.96f), Vector2.zero, Vector2.zero);

        Button topCloseButton = CreateButton("Top Close Button", panelRoot.transform, "X", 30f);
        SetRect(topCloseButton.GetComponent<RectTransform>(), new Vector2(0.9f, 0.88f), new Vector2(0.965f, 0.96f), Vector2.zero, Vector2.zero);
        topCloseButton.onClick.AddListener(Hide);

        pageRoots = new[]
        {
            CreateIntroPage(),
            CreateTutorialPage("Movement Window", "MOVEMENT", movementText, movementImage, movementScreenshot, "01"),
            CreateTutorialPage("Interact Window", "INTERACT", interactText, interactImage, interactScreenshot, "02"),
            CreateTutorialPage("Objective Window", "OBJECTIVE", objectiveText, objectiveImage, objectiveScreenshot, "03")
        };

        backButton = CreateButton("Back Button", panelRoot.transform, "<", 38f);
        SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.06f), new Vector2(0.17f, 0.16f), Vector2.zero, Vector2.zero);
        backButton.onClick.AddListener(ShowPreviousPage);

        nextButton = CreateButton("Next Button", panelRoot.transform, ">", 38f);
        SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.83f, 0.06f), new Vector2(0.93f, 0.16f), Vector2.zero, Vector2.zero);
        nextButton.onClick.AddListener(ShowNextPage);

        bottomCloseButton = CreateButton("Bottom Close Button", panelRoot.transform, "CLOSE", 26f);
        SetRect(bottomCloseButton.GetComponent<RectTransform>(), new Vector2(0.39f, 0.06f), new Vector2(0.61f, 0.16f), Vector2.zero, Vector2.zero);
        bottomCloseButton.onClick.AddListener(Hide);

        pageCounterLabel = CreateText("Page Counter", panelRoot.transform, "1 / 4", 24f, FontStyles.Bold);
        pageCounterLabel.color = new Color(0.58f, 1f, 1f, 1f);
        SetRect(pageCounterLabel.rectTransform, new Vector2(0.42f, 0.16f), new Vector2(0.58f, 0.2f), Vector2.zero, Vector2.zero);

        ShowPage(0);
    }

    private GameObject CreateIntroPage()
    {
        GameObject page = CreateUIObject("Explorer Welcome Window", panelRoot.transform);
        Image pageImage = page.AddComponent<Image>();
        pageImage.sprite = imageFrameSprite != null ? imageFrameSprite : panelSprite;
        pageImage.type = Image.Type.Sliced;
        pageImage.color = new Color(0.03f, 0.2f, 0.3f, 0.86f);
        SetRect(page.GetComponent<RectTransform>(), new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.84f), Vector2.zero, Vector2.zero);

        CreateStarField(page.transform);
        CreateSignalHexagon(page.transform);
        CreateHudLine(page.transform, new Vector2(0.18f, 0.78f), new Vector2(0.82f, 0.81f), new Color(0.24f, 1f, 1f, 0.32f));
        CreateHudLine(page.transform, new Vector2(0.24f, 0.23f), new Vector2(0.76f, 0.255f), new Color(0.24f, 1f, 1f, 0.24f));

        TextMeshProUGUI greeting = CreateText("Greeting", page.transform, "HELLO, EXPLORER", 58f, FontStyles.Bold);
        greeting.color = new Color(0.72f, 1f, 1f, 1f);
        SetRect(greeting.rectTransform, new Vector2(0.12f, 0.61f), new Vector2(0.88f, 0.76f), Vector2.zero, Vector2.zero);

        TextMeshProUGUI body = CreateText("Intro Body", page.transform, introText, 34f, FontStyles.Normal);
        body.color = Color.white;
        body.alignment = TextAlignmentOptions.Center;
        body.enableWordWrapping = true;
        body.lineSpacing = 8f;
        SetRect(body.rectTransform, new Vector2(0.22f, 0.34f), new Vector2(0.78f, 0.58f), Vector2.zero, Vector2.zero);

        TextMeshProUGUI prompt = CreateText("Continue Prompt", page.transform, "PRESS E OR CLICK  >  TO CONTINUE", 24f, FontStyles.Bold);
        prompt.color = new Color(0.42f, 0.95f, 1f, 0.92f);
        SetRect(prompt.rectTransform, new Vector2(0.24f, 0.15f), new Vector2(0.76f, 0.24f), Vector2.zero, Vector2.zero);

        return page;
    }

    private GameObject CreateTutorialPage(string objectName, string pageTitle, string body, Sprite imageSprite, Texture2D screenshotTexture, string pageNumber)
    {
        GameObject page = CreateUIObject(objectName, panelRoot.transform);
        Image pageImage = page.AddComponent<Image>();
        pageImage.sprite = imageFrameSprite != null ? imageFrameSprite : panelSprite;
        pageImage.type = Image.Type.Sliced;
        pageImage.color = new Color(0.05f, 0.28f, 0.34f, 0.8f);
        SetRect(page.GetComponent<RectTransform>(), new Vector2(0.07f, 0.2f), new Vector2(0.93f, 0.84f), Vector2.zero, Vector2.zero);

        TextMeshProUGUI titleLabel = CreateText("Page Title", page.transform, pageTitle, 40f, FontStyles.Bold);
        titleLabel.color = new Color(0.62f, 1f, 1f, 1f);
        titleLabel.alignment = TextAlignmentOptions.Left;
        SetRect(titleLabel.rectTransform, new Vector2(0.06f, 0.82f), new Vector2(0.78f, 0.95f), Vector2.zero, Vector2.zero);

        TextMeshProUGUI numberLabel = CreateText("Page Number", page.transform, pageNumber, 32f, FontStyles.Bold);
        numberLabel.color = new Color(0.42f, 0.95f, 1f, 0.88f);
        numberLabel.alignment = TextAlignmentOptions.Right;
        SetRect(numberLabel.rectTransform, new Vector2(0.78f, 0.82f), new Vector2(0.94f, 0.95f), Vector2.zero, Vector2.zero);

        CreateImageSlot(page.transform, imageSprite, screenshotTexture);

        TextMeshProUGUI bodyLabel = CreateText("Body", page.transform, body, 34f, FontStyles.Normal);
        bodyLabel.color = Color.white;
        bodyLabel.alignment = TextAlignmentOptions.TopLeft;
        bodyLabel.enableWordWrapping = true;
        bodyLabel.lineSpacing = 8f;
        SetRect(bodyLabel.rectTransform, new Vector2(0.57f, 0.16f), new Vector2(0.94f, 0.75f), Vector2.zero, Vector2.zero);

        return page;
    }

    private void CreateImageSlot(Transform parent, Sprite imageSprite, Texture2D screenshotTexture)
    {
        GameObject frame = CreateUIObject("Image Slot", parent);
        Image frameImage = frame.AddComponent<Image>();
        frameImage.sprite = imageFrameSprite != null ? imageFrameSprite : buttonSprite;
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.08f, 0.48f, 0.58f, 0.66f);
        SetRect(frame.GetComponent<RectTransform>(), new Vector2(0.06f, 0.13f), new Vector2(0.52f, 0.78f), Vector2.zero, Vector2.zero);

        frame.AddComponent<RectMask2D>();

        if (screenshotTexture != null)
        {
            GameObject screenshotObject = CreateUIObject("Assigned Tutorial Screenshot", frame.transform);
            RawImage screenshot = screenshotObject.AddComponent<RawImage>();
            screenshot.texture = screenshotTexture;
            screenshot.color = Color.white;
            SetRect(screenshot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(18f, 16f), new Vector2(-18f, -16f));
            screenshotObject.AddComponent<RawImageCropToFill>().Configure(screenshot);
            return;
        }

        GameObject imageObject = CreateUIObject("Assigned Tutorial Image", frame.transform);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = imageSprite;
        image.preserveAspect = true;
        image.color = imageSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        SetRect(image.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(18f, 16f), new Vector2(-18f, -16f));

        if (imageSprite != null)
            return;

        TextMeshProUGUI placeholder = CreateText("Image Placeholder", frame.transform, "IMAGE SLOT\nAssign Sprite", 30f, FontStyles.Bold);
        placeholder.color = new Color(0.72f, 1f, 1f, 0.86f);
        placeholder.alignment = TextAlignmentOptions.Center;
        SetRect(placeholder.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void CreateStarField(Transform parent)
    {
        for (int index = 0; index < 28; index++)
        {
            float x = 0.08f + Mathf.Repeat(index * 0.173f, 0.84f);
            float y = 0.14f + Mathf.Repeat(index * 0.291f, 0.72f);
            float size = 3f + index % 4 * 1.5f;
            float alpha = 0.2f + index % 5 * 0.08f;

            GameObject star = CreateUIObject($"Star {index + 1}", parent);
            Image starImage = star.AddComponent<Image>();
            starImage.color = new Color(0.72f, 1f, 1f, alpha);
            starImage.raycastTarget = false;
            SetPointRect(star.GetComponent<RectTransform>(), new Vector2(x, y), new Vector2(size, size));
        }
    }

    private void CreateSignalHexagon(Transform parent)
    {
        HolographicHexagonGraphic outer = CreateHexagon("Signal Hex Outer", parent, new Vector2(0.5f, 0.46f), new Vector2(310f, 310f));
        outer.OutlineOnly = true;
        outer.OutlineThickness = 10f;
        outer.color = new Color(0.3f, 1f, 1f, 0.22f);

        HolographicHexagonGraphic middle = CreateHexagon("Signal Hex Middle", parent, new Vector2(0.5f, 0.46f), new Vector2(220f, 220f));
        middle.OutlineOnly = true;
        middle.OutlineThickness = 6f;
        middle.color = new Color(0.55f, 1f, 1f, 0.34f);

        HolographicHexagonGraphic core = CreateHexagon("Signal Hex Core", parent, new Vector2(0.5f, 0.46f), new Vector2(92f, 92f));
        core.OutlineOnly = false;
        core.color = new Color(0.18f, 0.92f, 1f, 0.16f);
    }

    private HolographicHexagonGraphic CreateHexagon(string objectName, Transform parent, Vector2 anchor, Vector2 size)
    {
        GameObject hexagonObject = CreateUIObject(objectName, parent);
        HolographicHexagonGraphic hexagon = hexagonObject.AddComponent<HolographicHexagonGraphic>();
        hexagon.raycastTarget = false;
        SetPointRect(hexagonObject.GetComponent<RectTransform>(), anchor, size);
        return hexagon;
    }

    private void CreateHudLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject line = CreateUIObject("HUD Scan Line", parent);
        Image lineImage = line.AddComponent<Image>();
        lineImage.color = color;
        lineImage.raycastTarget = false;
        SetRect(line.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
    }

    private void ShowPreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }

    private void ShowNextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    private void ShowPage(int pageIndex)
    {
        if (pageRoots == null || pageRoots.Length == 0)
            return;

        currentPageIndex = Mathf.Clamp(pageIndex, 0, pageRoots.Length - 1);

        for (int index = 0; index < pageRoots.Length; index++)
        {
            if (pageRoots[index] != null)
                pageRoots[index].SetActive(index == currentPageIndex);
        }

        bool isFirstPage = currentPageIndex == 0;
        bool isLastPage = currentPageIndex == pageRoots.Length - 1;

        if (backButton != null)
            backButton.gameObject.SetActive(!isFirstPage);

        if (nextButton != null)
            nextButton.gameObject.SetActive(!isLastPage);

        if (bottomCloseButton != null)
            bottomCloseButton.gameObject.SetActive(isLastPage);

        if (pageCounterLabel != null)
            pageCounterLabel.text = $"{currentPageIndex + 1} / {pageRoots.Length}";
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

    private Button CreateButton(string objectName, Transform parent, string labelText, float labelSize)
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

        TextMeshProUGUI label = CreateText("Label", buttonObject.transform, labelText, labelSize, FontStyles.Bold);
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

    private void SetPointRect(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
