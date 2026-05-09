using UnityEngine;

// Author: David Haddad - CS480 design-patterns mushroom puzzle (May 2026)
// Observer pattern: typed ScriptableObject channel. Reuses the generic EventChannelSO<T>.

[CreateAssetMenu(fileName = "MushroomEventChannel", menuName = "Events/Mushroom Event Channel")]
public class MushroomEventChannelSO : EventChannelSO<MushroomActivationData> { }
