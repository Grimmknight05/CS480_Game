using UnityEngine;

[CreateAssetMenu(menuName = "Events/Fuel State Channel", fileName = "FuelState.channel")]
public class FuelStateChannel : EventChannelSO<FuelState>
{
}

public struct FuelState
{
    public int collected;
    public int target;

    public FuelState(int collected, int target)
    {
        this.collected = collected;
        this.target = target;
    }
}
