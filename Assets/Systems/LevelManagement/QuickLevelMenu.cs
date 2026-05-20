using UnityEngine;
public class QuickLevelMenu : MonoBehaviour
{
    public WorldSO hubWorld;
    public WorldSO level1;
    public WorldSO level2;

    public void LoadHub() => LevelManager.Instance.LoadWorld(hubWorld);
    public void LoadLevel1() => LevelManager.Instance.LoadWorld(level1);
    public void LoadLevel2() => LevelManager.Instance.LoadWorld(level2);
}