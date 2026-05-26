using UnityEngine;

public class RotatingShield : MonoBehaviour
{
    public float rotateSpeed = 30f;
    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}