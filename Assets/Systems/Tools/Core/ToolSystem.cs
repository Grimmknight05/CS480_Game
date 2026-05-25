using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;


// Manages tool usage and switching for the player.
// Attach to player
// Adding IAimContext allows tools to know where to aim (the camera) vs where to fire from (the usePoint).
public class ToolSystem : MonoBehaviour, IAimContext
{

    [Header("Aiming Setup")]
    [Tooltip("The camera determining what we are logically aiming at.")]
    [SerializeField] private Transform aimSource;
    
    [Tooltip("The physical barrel of the gun on the character model.")]
    [SerializeField] private Transform visualFirePoint;

    // 2. Fulfill the interface contract so weapons can read these variables
    public Transform AimSource => aimSource != null ? aimSource : transform;
    public Transform VisualFirePoint => visualFirePoint != null ? visualFirePoint : transform;
    
    [SerializeField] private Tool[] tools;
    private List<Tool> originalTools;
    private int tempWeaponSlot = -1;
    [SerializeField] private Transform usePoint;
    [SerializeField] private AudioSource audioSource;

    [Header("Dialogue")]
    [SerializeField] private DialogueEventChannelSO dialogueStartChannel;
    [SerializeField] private DialogueEndedChannelSO dialogueEndedChannel;
    private bool inputEnabled = true;

    private int currentToolIndex = 0;
    private PlayerInput playerInput;
    private PlayerControllerRefactored playerController;
    private InputAction useAction;
    private InputAction switchToolAction;
    private float[] lastUseTimes;

    private void OnEnable()
    {
        if (dialogueStartChannel != null) dialogueStartChannel.OnRaised += HandleDialogueStart;
        if (dialogueEndedChannel != null) dialogueEndedChannel.OnRaised += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        if (dialogueStartChannel != null) dialogueStartChannel.OnRaised -= HandleDialogueStart;
        if (dialogueEndedChannel != null) dialogueEndedChannel.OnRaised -= HandleDialogueEnded;
    }

    private void HandleDialogueStart(DialogueSO _) { inputEnabled = false; }
    private void HandleDialogueEnded() { inputEnabled = true; }
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerControllerRefactored>();

        if (usePoint == null)
        {
            usePoint = transform;
        }

        lastUseTimes = new float[tools.Length];
        for (int i = 0; i < lastUseTimes.Length; i++)
        {
            lastUseTimes[i] = -999f;
        }
        originalTools = new List<Tool>(tools);
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
        if (!inputEnabled) return;
        if (!attackInput.isPressed) return;

        if (playerController != null)
        {
            playerController.QueueCommand(new UseToolCommand(this, currentToolIndex));
        }
        else
        {
            UseTool();
        }
    }
    void OnSwitchTool(InputValue switchInput)
    {
        if (!inputEnabled) return;
        Debug.Log("Switch Tool");
        SwitchTool();
    }
    private void Update()
    {
        if (!inputEnabled) return;
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
        if (!IsToolReady(currentToolIndex)) return;
        UseToolBySlot(currentToolIndex);
    }

    /// <summary>
    /// Cooldown + bounds check for a specific slot. Used by UseToolCommand.CanExecute
    /// so the input buffer can hold the command until the cooldown clears.
    /// </summary>
    public bool IsToolReady(int slot)
    {
        if (slot < 0 || slot >= tools.Length) return false;
        Tool tool = tools[slot];
        if (tool == null) return false;
        return Time.time >= lastUseTimes[slot] + tool.cooldown;
    }

    /// <summary>
    /// Fires a specific slot. Assumes the caller already gated on IsToolReady().
    /// </summary>
    public void UseToolBySlot(int slot)
    {
        Tool tool = tools[slot];
        lastUseTimes[slot] = Time.time;
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
    public void AddTemporaryWeapon(Tool tempWeapon)
    {
        // If we already have a temp weapon, remove it first
        RemoveTemporaryWeapon();

        // Create new array with one extra slot
        Tool[] newTools = new Tool[tools.Length + 1];
        for (int i = 0; i < tools.Length; i++)
            newTools[i] = tools[i];
        newTools[tools.Length] = tempWeapon;
        tools = newTools;

        tempWeaponSlot = tools.Length - 1;

        // Optionally auto-switch to the new weapon
        currentToolIndex = tempWeaponSlot;
        Debug.Log($"[ToolSystem] Temporary weapon '{tempWeapon.toolName}' added.");
    }

    public void RemoveTemporaryWeapon()
    {
        if (tempWeaponSlot == -1) return;

        // Restore original tools
        tools = originalTools.ToArray();
        tempWeaponSlot = -1;

        // Reset current index if it was pointing to the removed slot
        if (currentToolIndex >= tools.Length)
            currentToolIndex = tools.Length - 1;
        if (currentToolIndex < 0) currentToolIndex = 0;

        Debug.Log("[ToolSystem] Temporary weapon removed.");
    }
}
