using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Puzzle/Stone Puzzle Config")]
public class StoneConfig : ScriptableObject
{
    public StoneRequirementTest[] requirements;
    // Optional: reference to what this puzzle unlocks (door, etc.)
    // But we'll keep events in the MonoBehaviour for flexibility
}