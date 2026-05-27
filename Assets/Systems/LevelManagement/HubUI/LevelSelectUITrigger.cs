using UnityEngine;

public class LevelSelectUITrigger : MonoBehaviour
{
    [SerializeField] private GameObject UIToTrigger;
    private bool activated = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            UIToTrigger.SetActive(true);
            activated = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIToTrigger.SetActive(false);
            activated = false;   // allow re‑triggering
        }
    }
    void ResetActivation()
    {
        activated = false;
    }

}
