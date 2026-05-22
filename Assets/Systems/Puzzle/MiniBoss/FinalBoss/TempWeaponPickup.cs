using UnityEngine;
using System;

public class TempWeaponPickup : MonoBehaviour
{
    public event Action<TempWeaponPickup> OnPickup;
    
    [Header("Visuals")]
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 2f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            isCollected = true;
            OnPickup?.Invoke(this);
        }
    }
}