// UIInputBlocker.cs
// A lightweight reference-counted gate that menus use to signal "a UI panel is
// open — stop processing player/camera input."
//
// Usage
//   Opening a panel:  UIInputBlocker.Push();
//   Closing a panel:  UIInputBlocker.Pop();
//   Checking:         UIInputBlocker.IsAnyUIOpen
//
// The reference count lets multiple overlapping panels (e.g. a pause menu on
// top of a fuel terminal) open and close independently without fighting each
// other.  Consumers (OrbitalCamera, PlayerControllerRefactored) simply check
// IsAnyUIOpen and skip their input logic when it is true.
//
// Added by: Katie Trinh

public static class UIInputBlocker
{
    private static int _openCount;

    /// <summary>True while at least one UI panel is open.</summary>
    public static bool IsAnyUIOpen => _openCount > 0;

    /// <summary>Call when a UI panel opens.</summary>
    public static void Push()
    {
        _openCount++;
    }

    /// <summary>Call when a UI panel closes.</summary>
    public static void Pop()
    {
        // Guard against mismatched calls (e.g. scene reloads) so the counter
        // never goes negative and permanently blocks input.
        if (_openCount > 0)
            _openCount--;
    }

    /// <summary>
    /// Hard-reset — call on scene unload or game restart to clear any
    /// leftover count from panels that were destroyed without calling Pop().
    /// </summary>
    public static void Reset() => _openCount = 0;
}
