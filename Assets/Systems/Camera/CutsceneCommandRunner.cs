using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// =====================================================================
// Command types for the cutscene system.
// =====================================================================

public enum CutsceneCommandType
{
    MoveToTransform,
    MoveToPosition,
    FollowTrack,
    LookAtTransform,
    SetFOV,
    SetDistance,
    Wait,
    WaitForEvent,
    RaiseEvent,
    EnableGameObject,
    DisableGameObject,
    InvokeUnityEvent,
}

[Serializable]
public class CutsceneCommand
{
    public CutsceneCommandType type;

    // Movement / position
    public Transform targetTransform;
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    public float duration = 1f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // Track following
    public Transform[] waypoints;
    public bool useSpline = false;

    // LookAt
    public Transform lookAtTarget;

    // Camera parameters
    public float targetFOV = 60f;
    public float targetDistance = 5f;

    // Flow / events – using your existing VoidEventChannelSO
    public float waitSeconds = 1f;
    public VoidEventChannelSO waitForChannel;
    public VoidEventChannelSO raiseChannel;

    // GameObjects / UnityEvents
    public GameObject targetGameObject;
    public UnityEvent onInvoke;
}

public class CutsceneCommandRunner : MonoBehaviour
{
    [SerializeField] private OrbitalCamera orbitalCamera;
    [SerializeField] private List<CutsceneCommand> commands = new List<CutsceneCommand>();

    private int currentIndex = -1;
    private float commandProgress = 0f;
    private bool isRunning = false;

    private Vector3 startCameraPos;
    private Quaternion startCameraRot;
    private float startFOV;

    public bool IsRunning => isRunning;

    void Awake()
    {
        if (orbitalCamera == null)
            orbitalCamera = FindObjectOfType<OrbitalCamera>();
    }

    public void StartCutscene()
    {
        if (isRunning) return;
        if (orbitalCamera == null)
        {
            Debug.LogError("CutsceneCommandRunner: No OrbitalCamera assigned.");
            return;
        }

        orbitalCamera.EnableExternalControl();
        isRunning = true;
        currentIndex = 0;
        commandProgress = 0f;
        BeginCurrentCommand();
    }

    public void StopCutscene()
    {
        if (!isRunning) return;
        isRunning = false;
        if (orbitalCamera != null)
            orbitalCamera.DisableExternalControl();
    }

    void Update()
    {
        if (!isRunning) return;
        if (currentIndex >= commands.Count)
        {
            StopCutscene();
            return;
        }

        CutsceneCommand cmd = commands[currentIndex];
        bool commandFinished = false;

        switch (cmd.type)
        {
            case CutsceneCommandType.MoveToTransform:
            case CutsceneCommandType.MoveToPosition:
            case CutsceneCommandType.SetFOV:
                commandProgress += Time.deltaTime / cmd.duration;
                float t = Mathf.Clamp01(commandProgress);
                float eased = cmd.easeCurve.Evaluate(t);
                UpdateContinuousCommand(cmd, eased);
                if (t >= 1f) commandFinished = true;
                break;

            case CutsceneCommandType.FollowTrack:
                commandProgress += Time.deltaTime / cmd.duration;
                float trackT = Mathf.Clamp01(commandProgress);
                UpdateFollowTrack(cmd, trackT);
                if (trackT >= 1f) commandFinished = true;
                break;

            case CutsceneCommandType.LookAtTransform:
                if (cmd.lookAtTarget != null)
                {
                    Vector3 direction = cmd.lookAtTarget.position - orbitalCamera.transform.position;
                    if (direction != Vector3.zero)
                        orbitalCamera.transform.rotation = Quaternion.LookRotation(direction);
                }
                commandFinished = true;
                break;

            case CutsceneCommandType.SetDistance:
                orbitalCamera.distance = Mathf.Clamp(cmd.targetDistance, orbitalCamera.minDistance, orbitalCamera.maxDistance);
                commandFinished = true;
                break;

            case CutsceneCommandType.Wait:
                commandProgress += Time.deltaTime;
                if (commandProgress >= cmd.waitSeconds)
                    commandFinished = true;
                break;

            case CutsceneCommandType.WaitForEvent:
                // The coroutine handles the wait; we just need to know if it's finished.
                // We'll use a flag set by the coroutine.
                if (waitForEventFinished) commandFinished = true;
                break;

            case CutsceneCommandType.RaiseEvent:
                if (cmd.raiseChannel != null)
                    cmd.raiseChannel.Raise();
                commandFinished = true;
                break;

            case CutsceneCommandType.EnableGameObject:
                if (cmd.targetGameObject != null)
                    cmd.targetGameObject.SetActive(true);
                commandFinished = true;
                break;

            case CutsceneCommandType.DisableGameObject:
                if (cmd.targetGameObject != null)
                    cmd.targetGameObject.SetActive(false);
                commandFinished = true;
                break;

            case CutsceneCommandType.InvokeUnityEvent:
                cmd.onInvoke?.Invoke();
                commandFinished = true;
                break;

            default:
                commandFinished = true;
                break;
        }

        if (commandFinished)
        {
            currentIndex++;
            commandProgress = 0f;
            waitForEventFinished = false; // reset for next command
            if (currentIndex < commands.Count)
                BeginCurrentCommand();
        }
    }

    private bool waitForEventFinished = false;

    private void BeginCurrentCommand()
    {
        if (currentIndex >= commands.Count) return;

        CutsceneCommand cmd = commands[currentIndex];
        waitForEventFinished = false;

        switch (cmd.type)
        {
            case CutsceneCommandType.MoveToTransform:
                startCameraPos = orbitalCamera.transform.position;
                startCameraRot = orbitalCamera.transform.rotation;
                break;
            case CutsceneCommandType.MoveToPosition:
                startCameraPos = orbitalCamera.transform.position;
                startCameraRot = orbitalCamera.transform.rotation;
                break;
            case CutsceneCommandType.SetFOV:
                if (orbitalCamera.GetComponentInChildren<Camera>() is Camera cam)
                    startFOV = cam.fieldOfView;
                break;
            case CutsceneCommandType.FollowTrack:
                startCameraPos = orbitalCamera.transform.position;
                startCameraRot = orbitalCamera.transform.rotation;
                break;
            case CutsceneCommandType.WaitForEvent:
                if (cmd.waitForChannel != null)
                    StartCoroutine(WaitForEventCoroutine(cmd.waitForChannel));
                else
                    waitForEventFinished = true; // skip if no channel
                break;
        }
    }

    private System.Collections.IEnumerator WaitForEventCoroutine(VoidEventChannelSO channel)
    {
        bool raised = false;
        System.Action handler = () => raised = true;
        channel.OnRaised += handler;
        yield return new WaitUntil(() => raised);
        channel.OnRaised -= handler;
        waitForEventFinished = true;
    }

    private void UpdateContinuousCommand(CutsceneCommand cmd, float t)
    {
        switch (cmd.type)
        {
            case CutsceneCommandType.MoveToTransform:
                if (cmd.targetTransform != null)
                {
                    orbitalCamera.transform.position = Vector3.Lerp(startCameraPos, cmd.targetTransform.position, t);
                    orbitalCamera.transform.rotation = Quaternion.Slerp(startCameraRot, cmd.targetTransform.rotation, t);
                }
                break;
            case CutsceneCommandType.MoveToPosition:
                Quaternion targetRot = Quaternion.Euler(cmd.targetRotation);
                orbitalCamera.transform.position = Vector3.Lerp(startCameraPos, cmd.targetPosition, t);
                orbitalCamera.transform.rotation = Quaternion.Slerp(startCameraRot, targetRot, t);
                break;
            case CutsceneCommandType.SetFOV:
                if (orbitalCamera.GetComponentInChildren<Camera>() is Camera cam)
                    cam.fieldOfView = Mathf.Lerp(startFOV, cmd.targetFOV, t);
                break;
        }
    }

    private void UpdateFollowTrack(CutsceneCommand cmd, float t)
    {
        if (cmd.waypoints == null || cmd.waypoints.Length < 2) return;

        Vector3 pos;
        Quaternion rot = orbitalCamera.transform.rotation;
        
        int i = 0;
        float frac = 0f;

        if (cmd.useSpline && cmd.waypoints.Length >= 4)
        {
            float segment = t * (cmd.waypoints.Length - 1);
            i = Mathf.FloorToInt(segment);
            frac = segment - i;
            i = Mathf.Clamp(i, 0, cmd.waypoints.Length - 2);
            pos = CatmullRom(
                cmd.waypoints[Mathf.Max(i-1, 0)].position,
                cmd.waypoints[i].position,
                cmd.waypoints[i+1].position,
                cmd.waypoints[Mathf.Min(i+2, cmd.waypoints.Length-1)].position,
                frac);
        }
        else
        {
            float segment = t * (cmd.waypoints.Length - 1);
            i = Mathf.FloorToInt(segment);
            frac = segment - i;
            i = Mathf.Clamp(i, 0, cmd.waypoints.Length - 2);
            pos = Vector3.Lerp(cmd.waypoints[i].position, cmd.waypoints[i+1].position, frac);
        }

        // Optional: face direction of movement
        if (cmd.waypoints.Length > 1 && t < 0.99f)
        {
            Vector3 nextPos;
            if (cmd.useSpline && cmd.waypoints.Length >= 4)
            {
                nextPos = GetSplinePointAt(cmd, t + 0.05f);
            }
            else
            {
                // Linear: use same i, frac but clamped to next segment
                float nextT = Mathf.Clamp01(t + 0.05f);
                float nextSegment = nextT * (cmd.waypoints.Length - 1);
                int nextI = Mathf.FloorToInt(nextSegment);
                float nextFrac = nextSegment - nextI;
                nextI = Mathf.Clamp(nextI, 0, cmd.waypoints.Length - 2);
                nextPos = Vector3.Lerp(cmd.waypoints[nextI].position, cmd.waypoints[nextI+1].position, nextFrac);
            }
            Vector3 dir = (nextPos - pos).normalized;
            if (dir != Vector3.zero)
                rot = Quaternion.LookRotation(dir);
        }

        orbitalCamera.transform.position = pos;
        orbitalCamera.transform.rotation = rot;
    }

    private Vector3 GetSplinePointAt(CutsceneCommand cmd, float t)
    {
        t = Mathf.Clamp01(t);
        float segment = t * (cmd.waypoints.Length - 1);
        int i = Mathf.FloorToInt(segment);
        float frac = segment - i;
        i = Mathf.Clamp(i, 0, cmd.waypoints.Length - 2);
        return CatmullRom(
            cmd.waypoints[Mathf.Max(i-1, 0)].position,
            cmd.waypoints[i].position,
            cmd.waypoints[i+1].position,
            cmd.waypoints[Mathf.Min(i+2, cmd.waypoints.Length-1)].position,
            frac);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) +
               (-p0 + p2) * t +
               (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
               (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}