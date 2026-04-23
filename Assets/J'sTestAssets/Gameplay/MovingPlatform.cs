using UnityEngine;

public class MovingPlatform : SignalListener
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 2f;

    private bool move;

    protected override void HandleSignal(string id, float value)
    {
        if (id != listenID) return;

        move = value >= 1f;
    }

    private void Update()
    {
        if (!move) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );
    }
}