using System.Collections;
using System.Collections.Generic;
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

    [Header("Planet Display")]
    public Transform planetContainer;
    public GameObject defaultPlanetPrefab;
    public Vector3 defaultPlanetLocalPosition = Vector3.zero;

    [Header("Tip System")]
    [SerializeField] private TipListSO defaultTipList;   // renamed to avoid confusion
    private TipListSO activeTipList;                     // will hold the one to use

    [Header("World Info Database")]
    [SerializeField] private List<WorldLoadingInfo> worldInfoList;

    [Header("Settings")]
    public float minDisplayTime = 1.5f;
    private static string targetSceneName;
    private static WorldSO targetWorld;

    void Start()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
            StartCoroutine(LoadTargetScene());
        else
            Debug.LogError("No target scene specified for LoadingScreenManager");
    }

    public static void LoadScene(string sceneName, WorldSO world = null)
    {
        targetSceneName = sceneName;
        targetWorld = world;
        SceneManager.LoadScene("LoadingScreen");
    }

    private WorldLoadingInfo GetInfoForWorld(WorldSO world)
    {
        if (world == null) return null;
        foreach (var info in worldInfoList)
            if (info.world == world)
                return info;
        return null;
    }

    private void SetupPlanetAndTips()
    {
        WorldLoadingInfo info = GetInfoForWorld(targetWorld);
        activeTipList = defaultTipList;
        GameObject planetToShow = defaultPlanetPrefab;
        Vector3 pos = defaultPlanetLocalPosition;
        //Vector3 rot = Vector3.zero;
        //Vector3 scale = Vector3.one;

        if (info != null)
        {
            if (info.planetPrefab != null)
                planetToShow = info.planetPrefab;
            if (info.tipList != null)
                activeTipList = info.tipList;
            pos = info.planetLocalPosition;
            //rot = info.planetLocalRotation;
            //scale = info.planetLocalScale;
        }

        if (planetToShow != null && planetContainer != null)
        {
            // Clear existing
            foreach (Transform child in planetContainer)
                Destroy(child.gameObject);

            GameObject planet = Instantiate(planetToShow, planetContainer);
            planet.transform.localPosition = pos;
            //planet.transform.localEulerAngles = rot;
            //planet.transform.localScale = scale;
        }
        else if (planetContainer != null)
        {
            planetContainer.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadTargetScene()
    {
        SetupPlanetAndTips();

        // Show random tip
        if (activeTipList != null && activeTipList.tips.Count > 0)
        {
            var tip = activeTipList.tips[Random.Range(0, activeTipList.tips.Count)];
            if (tipText != null) tipText.text = tip.tipText;
        }
        else if (tipText != null)
        {
            tipText.text = "Loading...";
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            if (progressBar != null) progressBar.size = progress;
            if (progressText != null) progressText.text = $"{(progress * 100):F0}%";

            if (asyncLoad.progress >= 0.9f && (Time.time - startTime) >= minDisplayTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        targetSceneName = null;
        targetWorld = null;
    }
}