public interface IPuzzleStateProvider
{
    bool TryGetBool(ActivatorID id, out bool value);
    bool TryGetFloat(ActivatorID id, out float value);
    bool TryGetMushroomColor(ActivatorID id, out MushroomColor value);
    bool TryGetMushroomColorArray(ActivatorID id, out MushroomColor[] value);
}
