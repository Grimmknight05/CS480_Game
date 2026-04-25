using UnityEngine;

[CreateAssetMenu(menuName = "Events/Fuel State Channel")]
public class FuelStateChannel : EventChannelSO<(int collected, int target)> { }
