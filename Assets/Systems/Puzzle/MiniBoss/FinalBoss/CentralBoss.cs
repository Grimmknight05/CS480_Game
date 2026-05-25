using UnityEngine;
using System.Collections;

public class CentralBoss : Boss, IDamageable
{
    [Header("Central Boss Setup")]
    [SerializeField] private CentralBossConfig centralConfig;
    [SerializeField] private FloatActivatorChannel bossHealthChannel;
    [SerializeField] private ActivatorID bossHealthID;
    
    [Header("Shield")]
    private ShieldState shieldState = ShieldState.Up;
    private GameObject shieldVisual;
    
    [Header("Attack")]
    private float lastAttackTime = -999f;
    private Transform playerTransform;
    
    private float currentBossHealth;
    
    public enum ShieldState
    {
        Up,         // Cannot damage
        Dropping,   // Transitioning down
        Down,       // Can damage (vulnerability window)
        Rising      // Transitioning up
    }
    
    public float GetMaxHealth() => centralConfig.maxHealth;
    public float GetCurrentHealth() => currentBossHealth;
    public float GetHealthPercent() => currentBossHealth / centralConfig.maxHealth;
    public bool IsDamageable => shieldState == ShieldState.Down;
    
    protected override void Start()
    {
        base.Start();
        
        if (centralConfig == null)
        {
            Debug.LogError("[CentralBoss] CentralBossConfig not assigned!");
            return;
        }
        
        currentBossHealth = centralConfig.maxHealth;
        
        // Find player
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
        
        CreateShieldVisual();
        BroadcastHealth();
    }
    
    private void CreateShieldVisual()
    {
        if (centralConfig.shieldPrefab != null)
        {
            shieldVisual = Instantiate(centralConfig.shieldPrefab, transform);
            shieldVisual.name = "ShieldVisual";
            UpdateShieldVisuals();
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (!isFightActive) return;
        
        transform.Rotate(Vector3.up, centralConfig.rotateSpeed * Time.deltaTime);
        UpdateFloating();
        UpdateShieldState();
        
        // Attack only when shield is down (vulnerability window)
        if (shieldState == ShieldState.Down && playerTransform != null)
            TryAttack();
    }
    
    private void UpdateFloating()
    {
        float newY = transform.position.y + Mathf.Sin(Time.time * centralConfig.floatSpeed) * centralConfig.floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    private void UpdateShieldState()
    {
        if (shieldVisual == null) return;
        
        switch (shieldState)
        {
            case ShieldState.Dropping:
                shieldVisual.transform.localScale = Vector3.Lerp(
                    shieldVisual.transform.localScale,
                    Vector3.zero,
                    centralConfig.shieldTransitionSpeed * Time.deltaTime
                );
                if (shieldVisual.transform.localScale.magnitude < 0.1f)
                {
                    shieldVisual.transform.localScale = Vector3.zero;
                    shieldState = ShieldState.Down;
                }
                break;
                
            case ShieldState.Rising:
                shieldVisual.transform.localScale = Vector3.Lerp(
                    shieldVisual.transform.localScale,
                    Vector3.one * centralConfig.shieldRadius * 2f,
                    centralConfig.shieldTransitionSpeed * Time.deltaTime
                );
                if (shieldVisual.transform.localScale.magnitude > centralConfig.shieldRadius * 1.9f)
                {
                    shieldVisual.transform.localScale = Vector3.one * centralConfig.shieldRadius * 2f;
                    shieldState = ShieldState.Up;
                }
                break;
        }
        
        UpdateShieldVisuals();
    }
    
    private void UpdateShieldVisuals()
    {
        if (shieldVisual == null) return;
        Renderer renderer = shieldVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color shieldColor = centralConfig.shieldColor;
            shieldColor.a = (shieldState == ShieldState.Up || shieldState == ShieldState.Rising) ? 0.3f : 0.1f;
            renderer.material.color = shieldColor;
        }
    }
    
    private void TryAttack()
    {
        if (Time.time < lastAttackTime + centralConfig.attackCooldown)
            return;
        
        lastAttackTime = Time.time;
        SpawnEnergyProjectile();
    }
    
    private void SpawnEnergyProjectile()
    {
        if (centralConfig.energyProjectilePrefab == null)
        {
            Debug.LogWarning("[CentralBoss] Energy projectile prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * 2f;
        GameObject projectileObj = Instantiate(centralConfig.energyProjectilePrefab, spawnPos, Quaternion.identity);
        
        BossEnergyProjectile projectile = projectileObj.GetComponent<BossEnergyProjectile>();
        if (projectile != null && playerTransform != null)
        {
            projectile.Initialize(playerTransform.position, centralConfig.projectileSpeed, centralConfig.projectileDamage);
        }
    }
    
    /// <summary>
    /// Lowers the shield, making the boss vulnerable.
    /// </summary>
    public void LowerShield()
    {
        if (shieldState == ShieldState.Up)
            shieldState = ShieldState.Dropping;
    }
    
    /// <summary>
    /// Raises the shield, protecting the boss.
    /// </summary>
    public void RaiseShield()
    {
        if (shieldState == ShieldState.Down)
            shieldState = ShieldState.Rising;
    }
    
    /// <summary>
    /// IDamageable implementation. Damage is only applied when shield is fully down.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (shieldState != ShieldState.Down)
            return;
        
        currentBossHealth -= amount;
        if (currentBossHealth < 0) currentBossHealth = 0;
        
        BroadcastHealth();
        StartCoroutine(FlashDamage());
        
        // Optional: If you want to auto-raise shield after a certain damage threshold, uncomment:
        // if (damageThisWindow >= centralConfig.maxDamagePerWindow) RaiseShield();
    }
    
    public void Heal(int amount)
    {
        currentBossHealth += amount;
        if (currentBossHealth > centralConfig.maxHealth)
            currentBossHealth = centralConfig.maxHealth;
        BroadcastHealth();
    }
    
    private IEnumerator FlashDamage()
    {
        if (shieldVisual == null) yield break;
        Renderer renderer = shieldVisual.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Color originalColor = renderer.material.color;
        Color hitColor = centralConfig.hitFlashColor;
        hitColor.a = originalColor.a;
        
        renderer.material.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }
    
    private void BroadcastHealth()
    {
        if (bossHealthChannel != null && bossHealthID != null)
        {
            float healthPercent = currentBossHealth / centralConfig.maxHealth;
            bossHealthChannel.RaiseEvent(bossHealthID, healthPercent);
        }
    }
}