using System;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateIntegrated : MonoBehaviour
{
    [Header("Puzzle System")]
    [SerializeField] private BoolActivatorChannel stateChannel;
    [SerializeField] private ActivatorID plateID;
    [SerializeField] private string[] acceptedTags = { "Pushable" };
    [SerializeField] private Transform visual;
    [SerializeField] private float pressedDrop = 0.08f;

    public event Action<PressurePlateIntegrated, bool> PressedChanged;
    public bool IsPressed => occupants.Count > 0;

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Vector3 visualUpLocalPos;

    void OnPressed()
    {
        if (visual != null)
            visual.localPosition = visualUpLocalPos + Vector3.down * pressedDrop;

        PressedChanged?.Invoke(this, true);

        if (stateChannel != null && plateID != null)
            stateChannel.RaiseEvent(plateID, true);
    }

    void OnReleased()
    {
        if (visual != null)
            visual.localPosition = visualUpLocalPos;

        PressedChanged?.Invoke(this, false);

        if (stateChannel != null && plateID != null)
            stateChannel.RaiseEvent(plateID, false);
    }

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void Awake()
    {
        if (visual != null) visualUpLocalPos = visual.localPosition;
    }

    void Start()
    {
        // Seed the validator's dictionary so TryGetBool never returns "not found"
        // for a plate that simply hasn't been stepped on yet. Runs after all
        // OnEnable calls (including the validator's channel subscription) but
        // before the first physics frame that would generate OnTriggerEnter.
        if (stateChannel != null && plateID != null)
            stateChannel.RaiseEvent(plateID, IsPressed);
        else
            Debug.LogWarning($"[PressurePlate] {name}: stateChannel or plateID is null — not wired for validation.", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsAccepted(other)) return;

        bool wasPressed = IsPressed;
        occupants.Add(other);
        if (!wasPressed && IsPressed) OnPressed();
    }

    void OnTriggerExit(Collider other)
    {
        if (!occupants.Remove(other)) return;

        if (!IsPressed) OnReleased();
    }

    bool IsAccepted(Collider other)
    {
        foreach (string t in acceptedTags)
            if (other.CompareTag(t)) return true;
        return false;
    }
}
