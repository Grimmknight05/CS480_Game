using UnityEngine;

public static class CursorHelper
{
    public static void Lock()
    {
        // Confined keeps the cursor invisible but lets the OS track its real
        // screen position, so UI buttons (e.g. the pause gear) remain clickable
        // via EventSystem raycasting.  Camera rotation uses mouse delta via the
        // Input System, which works identically in Confined and Locked modes.
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}