using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //might want to have player character refrencing gamecontroller to send game won event or not circular dependacies may be a problem with this
    [SerializeField] private PlayerController playerController; //Ref to player controller
    [SerializeField] private int pointsNeededToWin = 25; //Score to win
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] winSFX;
    [SerializeField] private AudioClip[] lossSFX;
    [SerializeField] private AudioClip[] gameStartSFX;

    //public delegate void WinDelegate();
    //public event WinDelegate OnGameWon; //A player won

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playRandomSFX(gameStartSFX);
        playerController.OnScoreChanged += CheckWinCondition; //Add CheckWinCondition function to list of things called when OnScoreChanged is invoked
        playerController.OnPlayerDeath += gameLoose;
    }

    private void playRandomSFX(AudioClip[] soundList)
    {
        int randomIndex = Random.Range(0, soundList.Length);
        audioSource.PlayOneShot(soundList[randomIndex]);
    }
    private void CheckWinCondition(int currentScore)
    {
        if (currentScore >= pointsNeededToWin)
        {
            gameWin();
        }
    }
    private void gameWin()
    {
        Debug.Log("Player wins!");
        //OnGameWon?.Invoke();//Invoke subscribers on won
        playerController.ShowWinScreen();
        playRandomSFX(winSFX);
        Time.timeScale = 0f; // Pause the game
        Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        //Add confetti
    }
    private void gameLoose()
    {
        Debug.Log("Better luck next time");
        playRandomSFX(lossSFX);
        Time.timeScale = 0f;
        //GameObject.FindGameObjectWithTag("Player").SetActive(false);
        //Add explosion

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
