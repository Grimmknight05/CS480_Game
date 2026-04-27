using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolHUD : MonoBehaviour
{
    [SerializeField] private ToolSystem toolSystem;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI toolNameText;
    [SerializeField] private Image toolIcon;

    void Start()
    {
        UpdateHUD();
    }

    void Update()
    {
        UpdateHUD(); // simple version (can optimize later)
    }

    private void UpdateHUD()
    {
        Tool current = toolSystem.GetCurrentTool();

        if (current == null)
            return;

        toolNameText.text = current.toolName;

        if (toolIcon != null)
            toolIcon.sprite = current.toolIcon;
    }
}