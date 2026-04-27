using UnityEngine;
using UnityEngine.InputSystem;

// Author: GitHub Copilot (weapon system manager, April 2026)

/// <summary>
/// Manages weapon switching and firing for the player.
/// Attach to player and assign weapon scriptable objects.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform firePoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private LayerMask enemyLayer;

    private int currentWeaponIndex = 0;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction switchWeaponAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Start()
    {
        if (playerInput != null)
        {
            attackAction = playerInput.actions.FindAction("Attack");
            switchWeaponAction = playerInput.actions.FindAction("SwitchWeapon");
        }

        if (weapons.Length == 0)
        {
            Debug.LogError("[WeaponSystem] No weapons assigned!");
        }
    }

    private void Update()
    {
        // Handle weapon switching
        if (switchWeaponAction != null && switchWeaponAction.triggered)
        {
            SwitchWeapon();
        }
    }

    /// <summary>
    /// Fire current weapon. Call this from input action or manually.
    /// </summary>
    public void Fire()
    {
        if (weapons.Length == 0)
            return;

        weapons[currentWeaponIndex].Fire(firePoint, audioSource, enemyLayer);
    }

    /// <summary>
    /// Switch to next weapon in array.
    /// </summary>
    public void SwitchWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        Debug.Log($"[WeaponSystem] Switched to: {weapons[currentWeaponIndex].toolName}");
    }

    /// <summary>
    /// Set specific weapon by index.
    /// </summary>
    public void SetWeapon(int index)
    {
        if (index >= 0 && index < weapons.Length)
        {
            currentWeaponIndex = index;
        }
    }

    /// <summary>
    /// Get current weapon.
    /// </summary>
    public Weapon GetCurrentWeapon()
    {
        return weapons[currentWeaponIndex];
    }

    /// <summary>
    /// Get all weapons.
    /// </summary>
    public Weapon[] GetAllWeapons()
    {
        return weapons;
    }
}
