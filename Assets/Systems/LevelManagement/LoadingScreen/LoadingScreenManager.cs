using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public Scrollbar progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;

    [Header("Tip System")]
    public TipListSO tipList;

    [Header("Settings")]
    public float minDisplayTime = 1.5f;

    private static string targetSceneName;

    void Start()
    {
        // Start loading the target scene immediately
        if (!string.IsNullOrEmpty(targetSceneName))
            StartCoroutine(LoadTargetScene());
        else
            Debug.LogError("No target scene specified for LoadingScreenManager");
    }

    public static void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        SceneManager.LoadScene("LoadingScreen"); // name of your loading scene
    }

    private IEnumerator LoadTargetScene()
    {
        // Show random tip
        if (tipList != null && tipList.tips.Count > 0)
        {
            var tip = tipList.tips[Random.Range(0, tipList.tips.Count)];
            if (tipText != null) tipText.text = tip.tipText;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"{(progress * 100):F0}%";

            if (asyncLoad.progress >= 0.9f && (Time.time - startTime) >= minDisplayTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // Target scene is now active, this LoadingScene is unloaded automatically
    }
}