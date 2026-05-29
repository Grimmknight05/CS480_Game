using UnityEngine;
using UnityEngine.Events;

public class AudioZone : MonoBehaviour
{
    [Tooltip("Invoked when the player enters this zone.")]
    [SerializeField] private UnityEvent OnPlayerEnter;

    [Tooltip("Invoked when the player exits this zone.")]
    [SerializeField] private UnityEvent OnPlayerExit;
    [Tooltip("Effect Transition time")]
    [SerializeField] private float transitionTime;
    [SerializeField] private AudioZoneChannelSO channel;
    [SerializeField] private float muffledVolume = 0.8f;
    [SerializeField] private float muffledCutoff = 500f;
    public float MuffledVolume => muffledVolume;
    public float TransitionTime => transitionTime;
    public float MuffledCutoff => muffledCutoff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            channel?.RaiseEvent(this, true);   // pass 'this' zone
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            channel?.RaiseEvent(this, false);
    }
}