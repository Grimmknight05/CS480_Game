using UnityEngine;

public class CameraControllor : MonoBehaviour
{
    public GameObject playerRef; //Player Referance
    //private Vector3 offset; //Camera Offset
    public Vector3 camOffset = new Vector3(0,10,-10); //Camera Offset from player
    private Vector3 playerLoc; //Used to store players location
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //offset = transform.position - PlayerRef.transform.position;I rather hard code this so cam will always have offset I need
    }

    // Update is called once per frame
    void LateUpdate()// called after all update functions are called
    {

        playerLoc = playerRef.transform.position; // Store PlayerRef's position in PlayerRef
        transform.position = playerLoc + camOffset; //Set parent cam's pos to the players location with an offset
    }
}
