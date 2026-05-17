using UnityEngine;

// Observer pattern: typed ScriptableObject channel for mushroom activations.
[CreateAssetMenu(fileName = "MushroomEventChannel", menuName = "Events/Mushroom Event Channel")]
public class MushroomEventChannelSO : EventChannelSO<MushroomActivationData>
{
}
