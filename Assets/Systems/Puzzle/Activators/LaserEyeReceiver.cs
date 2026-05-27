using UnityEngine;
using UnityEngine.Events;

public class LaserEyeReceiver : ResettableBehaviour
{
    [Header("Solution")]
    [SerializeField] private DoorLerp doorToOpen;
    [SerializeField] private bool closeDoorOnSolve;
    [SerializeField] private float requiredHoldTime = 0.25f;
    [SerializeField] private bool solveOnce = true;
    [SerializeField] private bool closeDoorWhenBeamLost;
    [SerializeField] private float beamLostGraceTime = 0.12f;

    [Header("Events")]
    [SerializeField] private UnityEvent onSolved;
    [SerializeField] private UnityEvent onBeamLostAfterSolved;

    [Header("Solved Visual")]
    [SerializeField] private Material materialOnSolve;

    private float currentHoldTime;
    private float lastLaserHitTime = float.NegativeInfinity;
    private bool solved;
    private Renderer[] cachedRenderers;
    private Material[][] startingMaterials;

    private void Awake()
    {
        CacheStartingMaterials();
    }

    public void ReceiveLaser()
    {
        lastLaserHitTime = Time.time;

        if (solved && solveOnce)
        {
            return;
        }

        currentHoldTime += Time.deltaTime;
        if (currentHoldTime >= requiredHoldTime)
        {
            Solve();
        }
    }

    private void Update()
    {
        if (Time.time - lastLaserHitTime <= beamLostGraceTime)
        {
            return;
        }

        currentHoldTime = 0f;

        if (solved && closeDoorWhenBeamLost)
        {
            solved = false;
            doorToOpen?.Close();
            onBeamLostAfterSolved?.Invoke();
        }
    }

    private void Solve()
    {
        if (solved) return;

        solved = true;
        if (closeDoorOnSolve)
        {
            doorToOpen?.Close();
        }
        else
        {
            doorToOpen?.Open();
        }
        onSolved?.Invoke();
        ApplySolvedVisual();
    }

    protected override void ResetInternal()
    {
        solved = false;
        currentHoldTime = 0f;
        lastLaserHitTime = float.NegativeInfinity;
        RestoreStartingMaterials();
    }

    private void CacheStartingMaterials()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        startingMaterials = new Material[cachedRenderers.Length][];

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            startingMaterials[i] = cachedRenderers[i] != null ? cachedRenderers[i].sharedMaterials : null;
        }
    }

    private void ApplySolvedVisual()
    {
        if (materialOnSolve == null)
        {
            return;
        }

        if (cachedRenderers == null)
        {
            CacheStartingMaterials();
        }

        foreach (Renderer cachedRenderer in cachedRenderers)
        {
            if (cachedRenderer == null)
            {
                continue;
            }

            Material[] solvedMaterials = cachedRenderer.sharedMaterials;
            if (solvedMaterials == null || solvedMaterials.Length == 0)
            {
                cachedRenderer.sharedMaterial = materialOnSolve;
                continue;
            }

            for (int i = 0; i < solvedMaterials.Length; i++)
            {
                solvedMaterials[i] = materialOnSolve;
            }

            cachedRenderer.sharedMaterials = solvedMaterials;
        }
    }

    private void RestoreStartingMaterials()
    {
        if (cachedRenderers == null || startingMaterials == null)
        {
            return;
        }

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] == null || startingMaterials[i] == null)
            {
                continue;
            }

            cachedRenderers[i].sharedMaterials = startingMaterials[i];
        }
    }
}
