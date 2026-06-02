using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RawImageCropToFill : MonoBehaviour
{
    private RawImage rawImage;
    private RectTransform rectTransform;

    public void Configure(RawImage image)
    {
        rawImage = image;
        rectTransform = image != null ? image.rectTransform : null;
        ApplyCrop();
    }

    private void Awake()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        rectTransform = rawImage != null ? rawImage.rectTransform : transform as RectTransform;
        ApplyCrop();
    }

    private void OnEnable()
    {
        ApplyCrop();
    }

    private void LateUpdate()
    {
        ApplyCrop();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyCrop();
    }

    private void ApplyCrop()
    {
        if (rawImage == null || rectTransform == null || rawImage.texture == null)
            return;

        float slotWidth = rectTransform.rect.width;
        float slotHeight = rectTransform.rect.height;
        if (slotWidth <= 0f || slotHeight <= 0f || rawImage.texture.height <= 0)
            return;

        float slotAspect = slotWidth / slotHeight;
        float imageAspect = rawImage.texture.width / (float)rawImage.texture.height;

        if (imageAspect > slotAspect)
        {
            float width = slotAspect / imageAspect;
            rawImage.uvRect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            return;
        }

        float height = imageAspect / slotAspect;
        rawImage.uvRect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
    }
}
