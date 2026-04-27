using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField] private float duration = 0.05f;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Fire(Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(gameObject, duration);
    }
}