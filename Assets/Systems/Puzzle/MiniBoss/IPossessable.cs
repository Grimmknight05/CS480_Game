// ============================================
// POSSESSION SYSTEM
// ============================================

/// <summary>
/// Interface for objects that can be possessed by the boss.
/// Provides a clean contract for interactive elements.
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

using System;
public interface IPossessable
{
    void OnPossessed();
    void OnReleasePossession();
    void Activate();
    void Deactivate();
    
    // Visual feedback (red glow)
    Material GetPossessionMaterial();
    bool IsPossessed { get; }
}

/// <summary>
/// Concrete implementation for doors, lasers, and electronic devices.
/// Handles visual state and activation logic.
/// </summary>
public class PossessableObject : MonoBehaviour, IPossessable
{
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material possessionMaterial; // Red glowing material
    [SerializeField] private float activationDelay = 0.5f;
    
    private MeshRenderer meshRenderer;
    private bool isPossessed;
    private Coroutine activationCoroutine;

    public bool IsPossessed => isPossessed;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnPossessed()
    {
        isPossessed = true;
        meshRenderer.material = possessionMaterial;
        OnActivationStart();
    }

    public void OnReleasePossession()
    {
        isPossessed = false;
        meshRenderer.material = normalMaterial;
        OnActivationEnd();
    }

    public void Activate()
    {
        if (!isPossessed) return;
        
        if (activationCoroutine != null)
            StopCoroutine(activationCoroutine);
            
        activationCoroutine = StartCoroutine(ActivationSequence());
    }

    public void Deactivate()
    {
        OnActivationEnd();
    }

    protected virtual void OnActivationStart() { }
    protected virtual void OnActivationEnd() { }

    private IEnumerator ActivationSequence()
    {
        yield return new WaitForSeconds(activationDelay);
        ExecuteActivation();
    }

    protected virtual void ExecuteActivation() { }

    public Material GetPossessionMaterial() => possessionMaterial;
}
