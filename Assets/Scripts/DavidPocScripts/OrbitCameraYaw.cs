using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCameraYaw : MonoBehaviour
{
    [SerializeField] private float sensitivity = 180f;
    [SerializeField] private bool requireRightMouse = false;

    private float yaw;

    void Start()
    {
        yaw = transform.localEulerAngles.y;
        if (!requireRightMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        Mouse m = Mouse.current;
        if (m == null) return;

        if (requireRightMouse && !m.rightButton.isPressed) return;

        float deltaX = m.delta.ReadValue().x;
        yaw += deltaX * sensitivity * Time.deltaTime * 0.01f;

        Vector3 e = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(e.x, yaw, e.z);
    }
}
