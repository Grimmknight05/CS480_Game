using UnityEngine;

public static class GameProgress
{
    private static PlayerSessionData _session;
    private static bool _isTestMode;

    // Lazy getter: creates a temporary session if none exists
    private static PlayerSessionData Session
    {
        get
        {
            if (_session == null)
            {
                _session = ScriptableObject.CreateInstance<PlayerSessionData>();
                _isTestMode = true;
                Debug.Log("GameProgress: Created temporary session (test mode).");
            }
            return _session;
        }
    }

    // Called by LevelManager in real game to replace the temporary session
    public static void UsePersistentSession(PlayerSessionData persistentSession)
    {
        if (persistentSession == null) return;
        _session = persistentSession;
        _isTestMode = false;
        Debug.Log("GameProgress: Switched to persistent session.");
    }

    public static bool IsAreaCompleted(AreaSO area)
    {
        return Session.IsAreaCompleted(area);
    }

    public static void CompleteArea(AreaSO area)
    {
        Session.CompleteArea(area);
    }

    public static bool TryGetCheckpoint(out Vector3 pos)
    {
        return Session.TryGetCheckpoint(out pos);
    }

    public static void SetCheckpoint(Vector3 pos)
    {
        Session.SetCheckpoint(pos);
    }

    public static void ClearCheckpoint()
    {
        Session.ClearCheckpoint();
    }
}