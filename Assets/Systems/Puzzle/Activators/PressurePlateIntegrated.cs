using System;
using System.Collections.Generic;
using UnityEngine;
public class PressurePlateIntegrated : MonoBehaviour
{
    [Header("Puzzle System")]
    [SerializeField] private ActivatorStateChannel stateChannel;
    [SerializeField] private string plateID = "pressure_plate_1";   // must match configuration
    [SerializeField] private string[] acceptedTags = { "Pushable" };
    [SerializeField] private Transform visual;
    [SerializeField] private float pressedDrop = 0.08f;

    public event Action<PressurePlateIntegrated, bool> PressedChanged;
    public bool IsPressed => occupants.Count > 0;

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Vector3 visualUpLocalPos;
    // ... existing code ...

    void OnPressed()
    {
        if (visual != null)
            visual.localPosition = visualUpLocalPos + Vector3.down * pressedDrop;

        PressedChanged?.Invoke(this, true);

        // Broadcast to the puzzle system
        if (stateChannel != null)
            stateChannel.RaiseEvent(plateID, true);
    }

    void OnReleased()
    {
        if (visual != null)
            visual.localPosition = visualUpLocalPos;

        PressedChanged?.Invoke(this, false);

        // Broadcast to the puzzle system
        if (stateChannel != null)
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