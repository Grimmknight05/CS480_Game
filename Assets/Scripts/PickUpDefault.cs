using UnityEngine;
using UnityEngine.Audio;

public class PickUpDefault : MonoBehaviour
{
    //PickUp BaseClass have other pickups inharet this behavoir
    public int points = 0;

    /* Sound */
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] pickUpSFX;//List of SFX

    //Despawn
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider[] colliders;

    private void playRandomSFX(AudioClip[] soundList)
    {
        int randomIndex = Random.Range(0, soundList.Length);
        if (soundList.Length > 0)//make sure there is a sound to play
        {
            audioSource.PlayOneShot(soundList[randomIndex]);
        }
    }
    public void onPickup()
    {
        Debug.Log("Pickup");
        playRandomSFX(pickUpSFX);
        foreach (var r in renderers)//Disable renderers
            if (r != null) r.enabled = false;

        foreach (var c in colliders)//Disable colliders
            if (c != null) c.enabled = false;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
