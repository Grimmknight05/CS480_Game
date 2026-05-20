using UnityEngine;

[DisallowMultipleComponent]
public class DialogueIconIndicator : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float bobHeight = 0.18f;
    [SerializeField] private float bobSpeed = 2.6f;
    [SerializeField] private float pulseAmount = 0.12f;
    [SerializeField] private float pulseSpeed = 3.4f;

    [Header("Facing")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Vector3 rotationOffsetEuler;

    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
    }

    private void Update()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        transform.localPosition = startLocalPosition + Vector3.up * bob;
        transform.localScale = startLocalScale * pulse;

        if (!faceCamera)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 toCamera = transform.position - camera.transform.position;
        if (toCamera.sqrMagnitude <= 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up)
            * Quaternion.Euler(rotationOffsetEuler);
    }

}
