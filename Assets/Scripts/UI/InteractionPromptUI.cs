using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private InteractionPromptChannelSO channel;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Transform playerTransform;

    private readonly Dictionary<Component, string> activePrompts = new();
    private readonly List<Component> destroyedKeys = new();

    void OnEnable()
    {
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
}
