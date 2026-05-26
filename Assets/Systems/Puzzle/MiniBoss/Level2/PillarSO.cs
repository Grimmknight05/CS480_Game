// PillarSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewPillarIdentifier", menuName = "Boss/Pillar Identifier")]
public class PillarSO : ScriptableObject
{
    // Just the asset itself acts as the unique key.
    // You can add a readable name if needed.
    [SerializeField] private string description;
}