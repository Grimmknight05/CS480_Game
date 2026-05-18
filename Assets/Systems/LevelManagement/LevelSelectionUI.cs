using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private LevelMetadata[] levelsByPlanet; // grouped by planet

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void Hide() => gameObject.SetActive(false);

    private void RefreshUI()
    {
        // Clear previous buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var level in levelsByPlanet)
        {
            bool unlocked = progressManager.IsLevelUnlocked(level);
            Button btn = Instantiate(levelButtonPrefab, buttonContainer).GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = level.displayName;
            btn.interactable = unlocked;
            if (unlocked)
                btn.onClick.AddListener(() => levelManager.LoadLevel(level));
        }
    }
}