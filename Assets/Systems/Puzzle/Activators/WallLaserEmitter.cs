using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WallLaserEmitter : MonoBehaviour
{
    [Header("Beam")]
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private bool isActive = false;
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
    //private bool laserLoop = false;
    private bool wasActiveLastFrame;
    private readonly List<Vector3> beamPoints = new();
    private LineRenderer lineRenderer;

    private Transform Origin => laserOrigin != null ? laserOrigin : transform;

    private void Awake()
    {
        EnsureLineRenderer();
        wasActiveLastFrame = isActive;
        
    }
    private void Start()
    {
        // Apply initial state
        if (isActive)
            ActivateLaser();
    }

    private void Update()
    {
        if (isActive != wasActiveLastFrame)
        {
            if (isActive)
                ActivateLaser();
            else
                DeactivateLaser();
            wasActiveLastFrame = isActive;
        }

        if (!isActive)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        TraceBeam();
    }
    private void ActivateLaser()
    {
        // Stop any leftover loop before starting a new one
        StopLoopSound();
        
        // Play start one-shot
        PlayOneShot(laserStartSFX);
        
        // Start the looping sound
        if (audioSource != null && laserLoopSFX != null)
        {
            Debug.Log("Laserloop");
            audioSource.clip = laserLoopSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Immediately trace the beam so the laser appears
        TraceBeam();
    }

    private void DeactivateLaser()
    {
        // Stop the loop sound
        StopLoopSound();
        
        // Play stop one-shot
        PlayOneShot(laserStopSFX);
        
        // Clear the line renderer
        lineRenderer.positionCount = 0;
    }
    private void StopLoopSound()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying && audioSource.loop)
            audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void SetActive(bool active)
    {
        if (isActive == active) return;   // no change
            isActive = active;
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer != null)
            return;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
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
