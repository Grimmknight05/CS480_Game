using UnityEngine;

// PlanetLevelSO.cs (extends WorldSO)
[CreateAssetMenu(menuName = "Level/PlanetLevel")]
public class PlanetLevelSO : WorldSO
{
    public int starsToEarn;//Test
    public string nextLevelScene;

    public override void OnLevelComplete(PlayerSessionData session)
    {
        base.OnLevelComplete(session);
        // Update session progress e.g., session.CompleteLevel(this);
        Debug.Log($"Planet {displayName} completed! Earned {starsToEarn} stars.");
    }
}