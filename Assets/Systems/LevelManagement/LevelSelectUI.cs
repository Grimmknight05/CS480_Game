using UnityEngine;
using UnityEngine.UI;
using TMPro; // optional, if using TextMeshPro
using System.Collections.Generic;

public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager; // drag from scene
    [SerializeField] private GameObject buttonPrefab;   // your button prefab
    [SerializeField] private Transform buttonContainer; // parent for buttons (e.g., ScrollView Content)

    [Header("UI Text Settings")]
    [SerializeField] private bool useTextMeshPro = false;
    [SerializeField] private string buttonTextFormat = "{0}"; // e.g., "{0} - {1}" for name + type
    
    [Header("Cursor Settings")]
    [SerializeField] private bool unlockCursorWhenVisible = true;
    [SerializeField] private bool hideCursorWhenLocked = true;
    private bool wasCursorLocked = false;

    private void Start()
    {
        if (levelManager == null)
            levelManager = FindAnyObjectByType<LevelManager>();

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
        // Clear existing buttons (optional, if you want to regenerate)
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Get all worlds from LevelManager
        // You'll need to expose the list in LevelManager – add a property
        List<WorldSO> worlds = levelManager.GetAllWorlds();

        foreach (WorldSO world in worlds)
        {
            // Instantiate button
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            
            // Set button text
            string displayText = string.Format(buttonTextFormat, world.displayName);
            if (useTextMeshPro)
            {
                TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = displayText;
            }
            else
            {
                Text legacyText = btnObj.GetComponentInChildren<Text>();
                if (legacyText != null) legacyText.text = displayText;
            }

            // Add click listener – load the level
            WorldSO capturedWorld = world; // important: capture local variable
            btn.onClick.AddListener(() => 
            {
                // Lock cursor before loading level
                CursorHelper.Lock();
                levelManager.LoadWorld(world, false);
            });
        }
    }
}