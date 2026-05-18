using UnityEngine;

public class HologramLevelSelector : MonoBehaviour
{
    [SerializeField] private LevelSelectionUI levelSelectionUI;

    // This method will be called from InteractableTrigger's UnityEvent
    public void ShowLevelSelection()
    {
        if (levelSelectionUI != null)
            levelSelectionUI.Show();
        else
            Debug.LogError("HologramLevelSelector: No LevelSelectionUI assigned.", this);
    }
}