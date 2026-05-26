using UnityEngine;
using System;

public class TempWeaponPickup : MonoBehaviour
{
    public event Action<TempWeaponPickup> OnPickup;
    
    [Header("Visuals")]
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Temp Weapon Data")]
    public TempWeapon tempWeaponSO;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (other.CompareTag("Player"))
        {
            isCollected = true;
            ToolSystem ts = other.GetComponent<ToolSystem>();
            if (ts != null && tempWeaponSO != null)
            {
                TempWeapon instance = Instantiate(tempWeaponSO);
                ts.EquipTempWeapon(instance);
            }
            OnPickup?.Invoke(this);
            Destroy(gameObject);
        }
    }
}