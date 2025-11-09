using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public float shootCooldown = 0.3f;
    public float projectileSpeed = 10f;
    
    Rigidbody2D rb;
    Vector2 input;
    float shootTimer;
    Camera mainCamera;
    
    [Header("Invincibility")]
    public float invincibilityDuration = 1f; // How long player is invincible after taking damage
    float invincibilityTimer = 0f;
    public bool isInvincible { get; private set; }
    
    [Header("Health Drain")]
    public float healthDrainRate = 0.2f; // 20% of max health per second while touching enemies
    System.Collections.Generic.HashSet<EnemyAI> touchingEnemies = new System.Collections.Generic.HashSet<EnemyAI>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Update invincibility timer
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            isInvincible = invincibilityTimer > 0f;
        }
        else
        {
            isInvincible = false;
        }
        
        // Don't process input if game is over or paused
        if (GameManager.I != null && (GameManager.I.isGameOver || GameManager.I.isPaused))
        {
            input = Vector2.zero;
            return;
        }
        
        // Drain health continuously while enemies are touching (even if invincible from initial hit)
        if (touchingEnemies.Count > 0 && GameManager.I != null && !GameManager.I.isGameOver && !GameManager.I.isPaused)
        {
            float maxHealth = GameManager.I.maxStability;
            float drainPerSecond = maxHealth * healthDrainRate; // 20% of max health per second
            float drainThisFrame = drainPerSecond * Time.deltaTime;
            GameManager.I.TakeDamage(drainThisFrame);
        }
        
        // Movement input
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        
        // Shooting
        shootTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && shootTimer <= 0f && projectilePrefab != null)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }
    
    public void TakeDamage()
    {
        // Start invincibility period
        invincibilityTimer = invincibilityDuration;
        isInvincible = true;
    }
    
    /// <summary>
    /// Register that an enemy is touching the player
    /// </summary>
    public void RegisterTouchingEnemy(EnemyAI enemy)
    {
        if (enemy != null)
        {
            touchingEnemies.Add(enemy);
        }
    }
    
    /// <summary>
    /// Unregister that an enemy is no longer touching the player
    /// </summary>
    public void UnregisterTouchingEnemy(EnemyAI enemy)
    {
        if (enemy != null)
        {
            touchingEnemies.Remove(enemy);
        }
    }

    void FixedUpdate()
    {
        // Don't move if game is over or paused
        if (GameManager.I != null && (GameManager.I.isGameOver || GameManager.I.isPaused))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        rb.linearVelocity = input * speed;
    }

    void Shoot()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        // Get mouse position in world space
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        
        // Calculate direction from player to mouse
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        // Set projectile velocity
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }
    }
}



