using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private InteractionPromptChannelSO channel;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Transform playerTransform;

    private readonly Dictionary<Component, string> activePrompts = new();
    private readonly List<Component> destroyedKeys = new();

    void Awake()
    {
        BuildFallbackPromptIfNeeded();
    }

    void OnEnable()
    {
        BuildFallbackPromptIfNeeded();

        if (channel != null) channel.OnRaised += HandlePrompt;
        if (panel != null) panel.SetActive(false);
    }

    void OnDisable()
    {
        if (channel != null) channel.OnRaised -= HandlePrompt;
        activePrompts.Clear();
    }

    private void HandlePrompt(InteractionPromptData data)
    {
        if (data.Source == null) return;

        if (data.Visible)
            activePrompts[data.Source] = data.Message;
        else
            activePrompts.Remove(data.Source);

        Refresh();
    }

    private void Refresh()
    {
        destroyedKeys.Clear();
        foreach (var kv in activePrompts)
        {
            if (kv.Key == null) destroyedKeys.Add(kv.Key);
        }
        foreach (var k in destroyedKeys) activePrompts.Remove(k);

        if (activePrompts.Count == 0)
        {
            if (panel != null) panel.SetActive(false);
            return;
        }

        Component chosen = null;
        if (playerTransform != null)
        {
            float bestSqr = float.PositiveInfinity;
            Vector3 playerPos = playerTransform.position;
            foreach (var kv in activePrompts)
            {
                float sqr = (kv.Key.transform.position - playerPos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    chosen = kv.Key;
                }
            }
        }
        else
        {
            foreach (var kv in activePrompts) { chosen = kv.Key; break; }
        }

        if (label != null) label.text = activePrompts[chosen];
        if (panel != null) panel.SetActive(true);
    }

    private void BuildFallbackPromptIfNeeded()
    {
        if (panel != null && label != null)
            return;

        GameObject panelObject = new GameObject("InteractionPromptPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.layer = gameObject.layer;
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 72f);
        panelRect.sizeDelta = new Vector2(440f, 64f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.12f, 0.16f, 0.85f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        labelObject.layer = gameObject.layer;
        labelObject.transform.SetParent(panelObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(18f, 6f);
        labelRect.offsetMax = new Vector2(-18f, -6f);

        TextMeshProUGUI promptLabel = labelObject.AddComponent<TextMeshProUGUI>();
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.color = new Color(0.78f, 1f, 1f, 1f);
        promptLabel.fontSize = 24f;
        promptLabel.fontStyle = FontStyles.Bold;
        promptLabel.raycastTarget = false;

        panel = panelObject;
        label = promptLabel;
    }
}
