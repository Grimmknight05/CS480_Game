using UnityEngine;

public class HUDBootstrap : MonoBehaviour
{
    private static HUDBootstrap instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
