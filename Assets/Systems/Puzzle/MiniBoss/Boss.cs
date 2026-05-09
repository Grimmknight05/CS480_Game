using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using System;



public abstract class Boss : MonoBehaviour
{
    [SerializeField] protected int activeCores = 3;
    protected int coresDestroyed;
    protected BossPhase currentPhase;
    protected BossState currentState;
    protected Dictionary<BossStateType, BossState> stateMap;

    public event Action<int> OnPhaseTransition;
    public event Action OnBossDefeated;

    protected virtual void Awake()
    {
        InitializeStates();
    }

    protected virtual void Start()
    {
        TransitionToState(BossStateType.Idle);
        InitializePhases(); // Derived classes must set phases array and call this.
        if (phases != null && phases.Length > 0)
        {
            currentPhase = phases[0];
            currentPhase.Initialize();
        }
    }

    protected virtual void Update()
    {
        currentState?.Update();
        currentPhase?.Update();
    }

    // Derived must implement
    protected abstract void InitializeStates();
    protected abstract void InitializePhases();
    protected abstract Bounds GetLevelBounds();

    protected virtual void HandlePhaseComplete()
    {
        coresDestroyed++;
        OnPhaseTransition?.Invoke(coresDestroyed);
        if (coresDestroyed >= activeCores)
            DefeatBoss();
        else
            AdvancePhase();
    }

    protected virtual void AdvancePhase()
    {
        int nextIndex = System.Array.IndexOf(phases, currentPhase) + 1;
        if (nextIndex < phases.Length)
        {
            currentPhase?.Cleanup();
            currentPhase = phases[nextIndex];
            currentPhase.Initialize();
            TransitionToState(BossStateType.Active);
        }
    }

    protected virtual void DefeatBoss()
    {
        TransitionToState(BossStateType.Defeated);
        OnBossDefeated?.Invoke();
        Cleanup();
    }

    public void TransitionToState(BossStateType newState)
    {
        currentState?.OnExit();
        currentState = stateMap[newState];
        currentState.OnEnter();
    }

    // Possession Interface
    public abstract void PossessObject(IPossessable target);
    public abstract void ReleasePossession(IPossessable target);

    protected virtual void Cleanup()
    {
        currentPhase?.Cleanup();
        foreach (var phase in phases) phase?.Cleanup();
    }

    protected BossPhase[] phases; // Set by derived class in InitializePhases()
}