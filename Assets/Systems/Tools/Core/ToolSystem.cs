using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolSystem : MonoBehaviour, IAimContext
{
    [Header("Aiming Setup")]
    [SerializeField] private Transform aimSource;
    [SerializeField] private Transform visualFirePoint;
    public Transform AimSource => aimSource != null ? aimSource : transform;
    public Transform VisualFirePoint => visualFirePoint != null ? visualFirePoint : transform;

    [Header("Tools")]
    [SerializeField] private Tool[] tools;
    [SerializeField] private Transform usePoint;
    [SerializeField] private AudioSource audioSource;

    [Header("Dialogue")]
    [SerializeField] private DialogueEventChannelSO dialogueStartChannel;
    [SerializeField] private DialogueEndedChannelSO dialogueEndedChannel;

    [Header("Temp Weapon")]
    [SerializeField] private Transform weaponMount;
    //[SerializeField] private TempWeaponUI tempWeaponUI;
    [SerializeField] private TempWeaponDropChannelSO tempWeaponDropChannel;

    private List<Tool> runtimeTools;
    private TempWeapon currentTempWeapon;    // separate slot
    private GameObject currentVisual;
    private int currentToolIndex = 0;
    private Dictionary<Tool, float> toolCooldownEndTimes = new Dictionary<Tool, float>();

    private PlayerInput playerInput;
    private PlayerControllerRefactored playerController;
    private InputAction useAction;
    private InputAction switchToolAction;
    private bool inputEnabled = true;

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

    private void HandleDialogueStart(DialogueSO _) => inputEnabled = false;
    private void HandleDialogueEnded() => inputEnabled = true;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerControllerRefactored>();
        if (usePoint == null) usePoint = transform;
        if (weaponMount == null) weaponMount = transform;
    }

    private void Start()
    {
        if (playerInput != null)
        {
            useAction = playerInput.actions.FindAction("Attack");
            switchToolAction = playerInput.actions.FindAction("SwitchWeapon");
        }

        // Instantiate runtime copies of normal tools
        runtimeTools = new List<Tool>();
        foreach (var toolSO in tools)
        {
            var instance = Instantiate(toolSO);
            if (instance is TempWeapon tempWep)
                tempWep.Initialize();   // Only TempWeapon has Initialize
            runtimeTools.Add(instance);
        }
        tools = runtimeTools.ToArray();

        currentToolIndex = 0;
        UpdateToolVisual(GetCurrentTool());
    }

    private void Update()
    {
        if (!inputEnabled) return;
        if (switchToolAction != null && switchToolAction.triggered)
            OnSwitchTool(default); // or call SwitchTool directly
    }

    // Input handler – must exist for the input system to find
    public void OnSwitchTool(InputValue value)
    {
        if (!inputEnabled) return;
        SwitchTool();
    }

    private void UpdateToolVisual(Tool tool)
    {
        if (currentVisual != null) Destroy(currentVisual);
        if (tool?.visualPrefab != null && weaponMount != null)
            currentVisual = Instantiate(tool.visualPrefab, weaponMount);
    }

    public Tool GetCurrentTool()
    {
        // Temp weapon overrides normal tools
        if (currentTempWeapon != null) return currentTempWeapon;
        if (runtimeTools != null && currentToolIndex >= 0 && currentToolIndex < runtimeTools.Count)
            return runtimeTools[currentToolIndex];
        return null;
    }

    public void SwitchTool()
    {
        if (currentTempWeapon != null)
        {
            // Drop temp weapon and exit – next switch will cycle normal tools
            DropTempWeapon();
            return;
        }

        if (runtimeTools.Count == 0) return;
        currentToolIndex = (currentToolIndex + 1) % runtimeTools.Count;
        UpdateToolVisual(runtimeTools[currentToolIndex]);
        Debug.Log($"[ToolSystem] Switched to: {runtimeTools[currentToolIndex].toolName}");
    }

    // ----- Cooldown and usage (public for commands) -----
    public bool IsToolReady(Tool tool)
    {
        if (tool == null) return false;
        if (!toolCooldownEndTimes.ContainsKey(tool)) return true;
        return Time.time >= toolCooldownEndTimes[tool];
    }

    public void UseTool(Tool tool)
    {
        if (!IsToolReady(tool)) return;
        tool.Use(usePoint, audioSource, tool.GetTargetLayer());
        toolCooldownEndTimes[tool] = Time.time + tool.cooldown;
    }

    // Slot-based methods for normal tools (kept for compatibility)
    public bool IsToolReady(int slot)
    {
        if (slot < 0 || slot >= runtimeTools.Count) return false;
        return IsToolReady(runtimeTools[slot]);
    }

    public void UseToolBySlot(int slot)
    {
        if (slot < 0 || slot >= runtimeTools.Count) return;
        UseTool(runtimeTools[slot]);
    }

    // ----- Temp Weapon Handling -----
    public void EquipTempWeapon(TempWeapon tempWeapon)
    {
        if (currentTempWeapon != null) DropTempWeapon();
        currentTempWeapon = tempWeapon;
        currentTempWeapon.Initialize();
        currentTempWeapon.OnAmmoDepleted += HandleTempWeaponAmmoEmpty;  // subscribe
        //if (tempWeaponUI != null) tempWeaponUI.SetTempWeapon(currentTempWeapon);
        UpdateToolVisual(currentTempWeapon);
    }
    private void HandleTempWeaponAmmoEmpty()
    {
        if (currentTempWeapon != null)
        {
            Debug.Log("[ToolSystem] Temp weapon ammo depleted – dropping.");
            DropTempWeapon();
        }
    }
    public void DropTempWeapon()
    {
        if (currentTempWeapon == null) return;
        // Unsubscribe to avoid memory leaks
        currentTempWeapon.OnAmmoDepleted -= HandleTempWeaponAmmoEmpty;
        currentTempWeapon = null;
        //if (tempWeaponUI != null) tempWeaponUI.SetTempWeapon(null);
        UpdateToolVisual(GetCurrentTool());
        tempWeaponDropChannel?.RaiseDrop();
    }

    // ----- Input handlers -----
    public void OnAttack(InputValue value)
    {
        if (!inputEnabled) return;
        if (!value.isPressed) return;
        Tool current = GetCurrentTool();
        if (current == null) return;
        if (playerController != null)
            playerController.QueueCommand(new UseToolCommand(this, current));
        else
            UseTool(current);
    }

    public void ResetAllCooldowns()
    {
        toolCooldownEndTimes.Clear();
    }
}