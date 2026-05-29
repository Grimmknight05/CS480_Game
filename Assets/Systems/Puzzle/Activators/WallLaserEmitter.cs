using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WallLaserEmitter : MonoBehaviour
{
    [Header("Beam")]
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float maxDistance = 60f;
    [SerializeField] private int maxRedirects = 4;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private float surfaceOffset = 0.04f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip laserStartSFX;
    [SerializeField] private AudioClip laserStopSFX;
    [SerializeField] private AudioClip laserLoopSFX;
    private bool laserLoop = false;

    private readonly List<Vector3> beamPoints = new();
    private LineRenderer lineRenderer;

    private Transform Origin => laserOrigin != null ? laserOrigin : transform;

    private void Awake()
    {
        EnsureLineRenderer();
    }

    private void Update()
    {
        if (!isActive)
        {
            lineRenderer.positionCount = 0;
            if (laserLoop)
            {
                if (audioSource != null && audioSource.isPlaying && audioSource.clip == laserLoopSFX)
                    audioSource.Stop();
                PlaySound(laserStopSFX);
                laserLoop = false;
            }
            return;
        }

        TraceBeam();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        EnsureLineRenderer();

        if (!isActive)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        if (isActive)
        {
        PlaySound(laserStartSFX);
            if (audioSource != null && laserLoopSFX != null)
            {
                audioSource.clip = laserLoopSFX;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        laserLoop = active;
        TraceBeam();
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer != null)
            return;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
    }
    private void PlaySound(AudioClip sound)
    {
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
    private void TraceBeam()
    {
        beamPoints.Clear();

        Vector3 origin = Origin.position;
        Vector3 direction = Origin.forward.normalized;
        beamPoints.Add(origin);

        for (int i = 0; i <= maxRedirects; i++)
        {
            if (TryGetBeamHit(origin, direction, out RaycastHit hit))
            {
                beamPoints.Add(hit.point);

                if (drawDebugRay)
                {
                    Debug.DrawRay(origin, direction * hit.distance, Color.red);
                }

                LaserEyeReceiver receiver = hit.collider.GetComponentInParent<LaserEyeReceiver>();
                if (receiver != null)
                {
                    receiver.ReceiveLaser();
                    break;
                }

                LaserRedirectorStone redirector = hit.collider.GetComponentInParent<LaserRedirectorStone>();
                if (redirector != null && redirector.TryGetRedirectDirection(direction, hit.normal, hit.point, out Vector3 redirectedDirection))
                {
                    origin = hit.point + redirectedDirection * surfaceOffset;
                    direction = redirectedDirection;
                    continue;
                }

                TurnableStone turnableStone = hit.collider.GetComponentInParent<TurnableStone>();
                if (turnableStone != null && TryGetTurnableStoneDirection(turnableStone, out Vector3 stoneDirection))
                {
                    origin = hit.point + stoneDirection * surfaceOffset;
                    direction = stoneDirection;
                    continue;
                }

                break;
            }

            beamPoints.Add(origin + direction * maxDistance);
            if (drawDebugRay)
            {
                Debug.DrawRay(origin, direction * maxDistance, Color.red);
            }
            break;
        }

        lineRenderer.positionCount = beamPoints.Count;
        lineRenderer.SetPositions(beamPoints.ToArray());
    }

    private bool TryGetBeamHit(Vector3 origin, Vector3 direction, out RaycastHit selectedHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, hitMask, triggerInteraction);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.collider.isTrigger && !HasBeamHandler(hit.collider))
            {
                continue;
            }

            selectedHit = hit;
            return true;
        }

        selectedHit = default;
        return false;
    }

    private bool HasBeamHandler(Collider hitCollider)
    {
        return hitCollider.GetComponentInParent<LaserEyeReceiver>() != null
            || hitCollider.GetComponentInParent<LaserRedirectorStone>() != null
            || hitCollider.GetComponentInParent<TurnableStone>() != null;
    }

    private bool TryGetTurnableStoneDirection(TurnableStone turnableStone, out Vector3 redirectedDirection)
    {
        redirectedDirection = turnableStone.transform.forward.normalized;
        return redirectedDirection.sqrMagnitude > 0.001f;
    }

    private void OnDrawGizmosSelected()
    {
        Transform originTransform = Origin;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(originTransform.position, originTransform.forward * 3f);
    }
}
