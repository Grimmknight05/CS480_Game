using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;



// Manages tool usage and switching for the player.
// Attach to player

public class ToolSystem : MonoBehaviour
{
    [SerializeField] private Tool[] tools;
    [SerializeField] private Transform usePoint;
    [SerializeField] private AudioSource audioSource;


    private int currentToolIndex = 0;
    private PlayerInput playerInput;
    private InputAction useAction;
    private InputAction switchToolAction;
    private float[] lastUseTimes;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (usePoint == null)
        {
            usePoint = transform;
        }

        lastUseTimes = new float[tools.Length];
        for (int i = 0; i < lastUseTimes.Length; i++)
        {
            lastUseTimes[i] = -999f;
        }
    }

    private void Start()
    {
        if (playerInput != null)
        {
            useAction = playerInput.actions.FindAction("Attack");
            switchToolAction = playerInput.actions.FindAction("SwitchWeapon");
        }

        if (tools.Length == 0)
        {
            Debug.LogError("[ToolSystem] No tools assigned!");
        }
    }
    void OnAttack(InputValue attackInput)
    {
        if (attackInput.isPressed)
        {
            Debug.Log("Use Tool");
            UseTool();
        }
    }
    private void Update()
    {
        if (switchToolAction != null && switchToolAction.triggered)
        {
            SwitchTool();
        }
    }

    /// <summary>
    /// Use current tool. Call this from input action or manually.
    /// </summary>
    public void UseTool()
    {
        if (tools.Length == 0)
            return;

        Tool tool = tools[currentToolIndex];

        if (Time.time < lastUseTimes[currentToolIndex] + tool.cooldown)
            return;

        lastUseTimes[currentToolIndex] = Time.time;

            tool.Use(usePoint, audioSource, tool.GetTargetLayer());
    }
        

    /// <summary>
    /// Switch to next tool in array.
    /// </summary>
    public void SwitchTool()
    {
        currentToolIndex = (currentToolIndex + 1) % tools.Length;
        Debug.Log($"[ToolSystem] Switched to: {tools[currentToolIndex].toolName}");
    }

    /// <summary>
    /// Set specific tool by index.
    /// </summary>
    public void SetTool(int index)
    {
        if (index >= 0 && index < tools.Length)
        {
            currentToolIndex = index;
            Debug.Log($"[ToolSystem] Selected: {tools[currentToolIndex].toolName}");
        }
    }

    /// <summary>
    /// Get current tool.
    /// </summary>
    public Tool GetCurrentTool()
    {
        return currentToolIndex < tools.Length ? tools[currentToolIndex] : null;
    }

    /// <summary>
    /// Get all tools.
    /// </summary>
    public Tool[] GetAllTools()
    {
        return tools;
    }

    /// <summary>
    /// Get current tool index.
    /// </summary>
    public int GetCurrentToolIndex()
    {
        return currentToolIndex;
    }

    /// <summary>
    /// Reset all tool cooldowns.
    /// </summary>
    public void ResetAllCooldowns()
    {
        for (int i = 0; i < lastUseTimes.Length; i++)
        {
            lastUseTimes[i] = -999f;
        }
    }
    public void ResetCooldown(int index)
    {
        if (index >= 0 && index < lastUseTimes.Length)
            lastUseTimes[index] = -999f;
    }
}
