using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Channels & Refs")]
    [SerializeField] private PlayerDeathChannelSO deathChannel;
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private PlayerSessionData sessionData;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject hud;

    [Header("Player")]
    [SerializeField] private PlayerInput playerInput;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void OnEnable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised += HandleDeath;

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    void OnDisable()
    {
        if (deathChannel != null)
            deathChannel.OnRaised -= HandleDeath;

        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);
    }

    void HandleDeath()
    {
        if (panel != null) panel.SetActive(true);
        if (hud != null) hud.SetActive(false);
        if (playerInput != null) playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnContinueClicked()
    {
        if (panel != null) panel.SetActive(false);
        if (hud != null) hud.SetActive(true);
        if (playerInput != null) playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameLoopManager != null)
            gameLoopManager.RequestContinue();
    }

    void OnRestartClicked()
    {
        if (sessionData != null)
            sessionData.ClearCheckpoint();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
