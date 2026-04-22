using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PressurePlate[] plates;
    [SerializeField] private DoorLerp door;
    [Tooltip("If true, the door stays open once solved. If false, releasing any plate re-closes it.")]
    [SerializeField] private bool latchOpen = true;

    private bool solved;

    void Start()
    {
        foreach (PressurePlate plate in plates)
            if (plate != null) plate.PressedChanged += OnPlateChanged;

        EvaluateState();
    }

    void OnDestroy()
    {
        foreach (PressurePlate plate in plates)
            if (plate != null) plate.PressedChanged -= OnPlateChanged;
    }

    void OnPlateChanged(PressurePlate plate, bool pressed)
    {
        EvaluateState();
    }

    void EvaluateState()
    {
        bool allPressed = true;
        foreach (PressurePlate plate in plates)
        {
            if (plate == null || !plate.IsPressed) { allPressed = false; break; }
        }

        if (allPressed && !solved)
        {
            solved = true;
            if (door != null) door.Open();
        }
        else if (!allPressed && solved && !latchOpen)
        {
            solved = false;
            if (door != null) door.Close();
        }
    }
}
