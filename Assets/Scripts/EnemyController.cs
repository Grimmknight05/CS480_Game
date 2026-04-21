using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EnemyController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player; //players transform
    private NavMeshAgent navMeshAgent;

    //Sound
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] enemySFX;
    private float soundTimer = 0f;
    [SerializeField] private float enemySoundMinInterval = 3f;//Plays soundFX every 3-5 seconds
    [SerializeField] private float enemySoundMaxInterval = 5f;
    //Max min

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playRandomSFX(enemySFX);
    }


    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            navMeshAgent.SetDestination(player.position);//navigate to player position //add team tags and make target closest from other team

            soundTimer -= Time.deltaTime;//CountDown soundTimer

            if (soundTimer <= 0f)
            {
                playRandomSFX(enemySFX);//Play SFX
                float enemySoundInterval = Random.Range(enemySoundMinInterval, enemySoundMaxInterval);//Random sound interval
                soundTimer = enemySoundInterval; //reset timer with interval
            }
            

        }
    }

    private void playRandomSFX(AudioClip[] soundList)
    {
        int randomIndex = Random.Range(0, soundList.Length);
        if(soundList.Length > 0)//make sure there is a sound to play
        {
            audioSource.PlayOneShot(soundList[randomIndex]);
        }
    }
}
