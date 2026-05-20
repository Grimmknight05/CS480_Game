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
    void ResetActivation()
    {
        activated = false;
    }

}
