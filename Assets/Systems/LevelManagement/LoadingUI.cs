using UnityEngine;
public class LoadingUI : MonoBehaviour
{
    void Start()
    {
        LevelManager.Instance.OnLevelLoadStarted += ShowLoading;
        LevelManager.Instance.OnLevelLoadCompleted += HideLoading;
    }

    void ShowLoading(WorldSO world) => gameObject.SetActive(true);
    void HideLoading(WorldSO world) => gameObject.SetActive(false);
}