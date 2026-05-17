using UnityEngine;

public class CameraCanary : MonoBehaviour
{
    private OrbitalCamera camScript;

    void Awake()
    {
        // This runs instantly, before Start(), and uses LogError to bypass Chrome filters
        Debug.LogError("CANARY IS AWAKE! THE SCRIPT SURVIVED THE BUILD!");
    }
    void Start()
    {
        camScript = GetComponent<OrbitalCamera>();
        // Shout into the console every 2 seconds
        InvokeRepeating("ReportStatus", 1f, 2f); 
    }

    void ReportStatus()
    {
        if (camScript == null)
        {
            Debug.LogError("CANARY: OrbitalCamera component is missing or destroyed!");
            return;
        }

        if (camScript.playerRef == null)
        {
            Debug.LogError("CANARY: playerRef is completely NULL in the build!");
        }
        else
        {
            Debug.Log("CANARY: Tracking " + camScript.playerRef.name + " at position " + camScript.playerRef.position);
        }
    }
}