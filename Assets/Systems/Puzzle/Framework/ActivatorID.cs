using UnityEngine;

[CreateAssetMenu(fileName = "NewActivatorID", menuName = "Puzzle/Activator ID")]
public class ActivatorID : ScriptableObject
{
    [SerializeField, TextArea] private string description;
}