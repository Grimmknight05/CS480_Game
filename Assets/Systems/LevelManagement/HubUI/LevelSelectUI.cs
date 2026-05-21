using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    [Header("UI Text Settings")]
    [SerializeField] private bool useTextMeshPro = false;
    [SerializeField] private string buttonTextFormat = "{0}";

    [Header("Level Filtering")]
    [SerializeField] private bool showHubWorld = false;

    private void Start()
    {
        if (levelManager == null)
            levelManager = FindFirstObjectByType<LevelManager>();

        if (levelManager == null)
        {
            Debug.LogError("LevelSelectUI: No LevelManager found!");
            return;
        }

        GenerateButtons();
    }

    private void OnEnable()
    {
        CursorHelper.Unlock();
    }

    private void OnDisable()
    {
        CursorHelper.Lock();
    }

    private void GenerateButtons()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        List<WorldSO> worlds = levelManager.GetAllWorlds();
        Debug.Log($"LevelSelectUI: Found {worlds.Count} worlds total.");

        int buttonCount = 0;
        foreach (WorldSO world in worlds)
        {
            if (!showHubWorld && world.isHubWorld)
            {
                Debug.Log($"Skipping hub world: {world.displayName}");
                continue;
            }

            Debug.Log($"Creating button for world: {world.displayName}, scene: {world.sceneName}");
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError($"Button prefab missing Button component on {btnObj.name}");
                continue;
            }

            // Set button text
            string displayText = string.Format(buttonTextFormat, world.displayName);
            Debug.Log($"Setting text for {world.displayName} to '{displayText}'");

            // Find all TMP components in the button hierarchy
            TextMeshProUGUI[] allTMP = btnObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"Found {allTMP.Length} TMP components in {btnObj.name}");

            if (allTMP.Length > 0)
            {
                // Use the first one (usually the most specific)
                allTMP[0].text = displayText;
                Debug.Log($"Assigned text to {allTMP[0].name}: {allTMP[0].text}");
            }
            else
            {
                Debug.LogError($"No TextMeshProUGUI found in button prefab '{btnObj.name}'. Check prefab hierarchy.");
            }

            // CRITICAL: capture the variable inside the loop
            WorldSO capturedWorld = world;
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"Loading world: {capturedWorld.displayName}");
                CursorHelper.Lock();
                levelManager.LoadWorld(capturedWorld, false);
            });

            buttonCount++;
        }

        Debug.Log($"LevelSelectUI: Generated {buttonCount} buttons.");
    }
}