// LevelManager.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("World List")]
    [SerializeField] private List<WorldSO> allWorlds;

    [Header("Dependencies")]
    [SerializeField] private PlayerSessionData playerSession;

    // Events for Observer pattern
    public event Action<WorldSO> OnLevelLoadStarted;
    public event Action<WorldSO> OnLevelLoadCompleted;
    public event Action<WorldSO> OnLevelCompleted;  // Triggered by level end


    public void RaiseLoadStarted(WorldSO world) => OnLevelLoadStarted?.Invoke(world);
    public void RaiseLoadCompleted(WorldSO world) => OnLevelLoadCompleted?.Invoke(world);
    public void RaiseLevelCompleted(WorldSO world) => OnLevelCompleted?.Invoke(world);
    // Returns the list of all worlds (read-only)
    public List<WorldSO> GetAllWorlds() => allWorlds;
    public GameObject PlayerPrefab;
    // State tracking
    private WorldSO currentWorld;
    public WorldSO CurrentWorld => currentWorld;
    private bool isLoading = false;
    //public PlayerSessionData Session => playerSession;

    // Queue for commands
    private Queue<ISceneCommand> commandQueue = new Queue<ISceneCommand>();
    private bool isExecutingCommands = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        PlayerSessionData.SetInstance(playerSession);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Start at hub if no current world
        if (currentWorld == null)
            LoadHub();
        GameProgress.UsePersistentSession(playerSession);
    }

    private void Update()
    {
        if (!isExecutingCommands && commandQueue.Count > 0)
            StartCoroutine(ExecuteCommandCoroutine(commandQueue.Dequeue()));
    }

    // ---- Public API ----
    public void LoadHub() => LoadWorld(GetHubWorld(), false);
    public void LoadLevel(string sceneName) => LoadWorld(GetWorldByScene(sceneName));

    public void LoadWorld(WorldSO world, bool keepCheckpoint = false)
    {
        if (world == null) return;
        if (isLoading) { Debug.Log("Already loading, queueing"); return; }

        if (!keepCheckpoint)
            Session.ClearCheckpoint();

        ExecuteCommand(new LoadWorldCommand(world));
    }
    public bool TryGetCurrentWorld(out WorldSO world)
    {
        world = currentWorld;
        return world != null;
    }
    // Called from within a level (e.g., goal trigger)
    public void CompleteCurrentLevel()
    {
        if (currentWorld == null || currentWorld.isHubWorld)
        {
            Debug.Log("Cannot complete hub world");
            return;
        }

        currentWorld.OnLevelComplete(playerSession);
        OnLevelCompleted?.Invoke(currentWorld);
        
        // Optional: save progress
        // Then return to hub or load next level
        Session.ClearCheckpoint();
        LoadHub();
    }

    // ---- Internal helpers ----
    private WorldSO GetHubWorld() => allWorlds.Find(w => w.isHubWorld);
    private WorldSO GetWorldByScene(string sceneName) => allWorlds.Find(w => w.sceneName == sceneName);

    internal void SetCurrentWorld(WorldSO world) => currentWorld = world;
    internal bool IsLoading => isLoading;
    internal PlayerSessionData Session => playerSession;

    // ---- Command queue system ----
    public void ExecuteCommand(ISceneCommand command) => commandQueue.Enqueue(command);

    private IEnumerator ExecuteCommandCoroutine(ISceneCommand command)
    {
        isExecutingCommands = true;
        yield return command.Execute(this);
        isExecutingCommands = false;
    }
}



