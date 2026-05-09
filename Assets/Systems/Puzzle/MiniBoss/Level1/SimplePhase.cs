using UnityEngine;

[System.Serializable]
public class SimplePhase : BossPhase
{
    [SerializeField] private string phaseName;
    [SerializeField] private float duration = 5f;   // wait this many seconds then complete
    private float timer;

    protected override void OnPhaseStart()
    {
        timer = duration;
        Debug.Log($"Phase {phaseName} started. Will complete in {duration} seconds.");
    }

    public override void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            CompletePhase();
        }
    }
}