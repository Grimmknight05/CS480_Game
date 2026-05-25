using UnityEngine;

[CreateAssetMenu(fileName = "CentralBossConfig", menuName = "Boss/Central Boss Config")]
public class CentralBossConfig : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 300f;
    public float maxDamagePerWindow = 100f; // Max damage before shield goes back up
    
    [Header("Movement")]
    public float rotateSpeed = 30f;
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1.5f;
    
    [Header("Shield")]
    public GameObject shieldPrefab;
    public float shieldRadius = 5f;
    public Color shieldColor = new Color(0.2f, 0.6f, 1f, 0.3f);
    public float shieldTransitionSpeed = 3f;
    
    [Header("Attack")]
    public GameObject energyProjectilePrefab;
    public float projectileSpeed = 5f;
    public int projectileDamage = 15;
    public float attackCooldown = 2f; // Attack every X seconds during vulnerability
    
    [Header("Visual Feedback")]
    public Color hitFlashColor = new Color(1f, 0.3f, 0.3f, 0.3f);
}
