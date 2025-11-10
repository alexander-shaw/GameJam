using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {
    [Header("Settings")]
    public float speed = 3f;
    public float health = 2f;
    public float damage = 15f; // Increased from 1 to 15-20 for faster death
    public float chaseRange = 5f; // Only chase player within this range.

    [Header("Drops")]
    public GameObject timeCrystalPrefab;

    Rigidbody2D rb;
    Transform player;
    float currentHealth;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = health;
    }

    void Start() {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null) {
            player = playerObj.transform;
        }
    }

    void FixedUpdate() {
        if (player != null) {
            Vector2 toPlayer = (player.position - transform.position);
            float sqrDist = toPlayer.sqrMagnitude;
            float sqrRange = chaseRange * chaseRange;

            if (sqrDist <= sqrRange) {
                // Move toward player
                Vector2 direction = toPlayer.normalized;
                rb.linearVelocity = direction * speed;
            } else {
                // Stop moving when player is out of range
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // public void TakeDamage(float amount)
    // {
    //     currentHealth -= amount;
    //     if (currentHealth <= 0f)
    //     {
    //         Die();
    //     }
    // }
    //
    // void Die()
    // {
    //     // Drop time crystal
    //     if (timeCrystalPrefab != null)
    //     {
    //         Instantiate(timeCrystalPrefab, transform.position, Quaternion.identity);
    //     }
    //     
    //     // Notify GameManager of enemy kill
    //     if (GameManager.I != null)
    //     {
    //         GameManager.I.OnEnemyKilled();
    //     }
    //     
    //     Destroy(gameObject);
    // }
    //
    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // Damage player on contact (only if player is not invincible)
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         PlayerController player = collision.gameObject.GetComponent<PlayerController>();
    //         if (player != null && !player.isInvincible)
    //         {
    //             if (GameManager.I != null)
    //             {
    //                 GameManager.I.TakeDamage(damage);
    //                 player.TakeDamage(); // Trigger invincibility frames
    //             }
    //         }
    //         
    //         // Register that we're touching the player (for continuous health drain)
    //         if (player != null)
    //         {
    //             player.RegisterTouchingEnemy(this);
    //         }
    //     }
    // }
    //
    // void OnCollisionStay2D(Collision2D collision)
    // {
    //     // Ensure we stay registered as touching (in case registration was missed)
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         PlayerController player = collision.gameObject.GetComponent<PlayerController>();
    //         if (player != null)
    //         {
    //             player.RegisterTouchingEnemy(this);
    //         }
    //     }
    // }
    //
    // void OnCollisionExit2D(Collision2D collision)
    // {
    //     // Unregister when we stop touching the player
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         PlayerController player = collision.gameObject.GetComponent<PlayerController>();
    //         if (player != null)
    //         {
    //             player.UnregisterTouchingEnemy(this);
    //         }
    //     }
    // }
    //
    // void OnDestroy()
    // {
    //     // Clean up: unregister from player if we're destroyed while touching
    //     GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    //     if (playerObj != null)
    //     {
    //         PlayerController player = playerObj.GetComponent<PlayerController>();
    //         if (player != null)
    //         {
    //             player.UnregisterTouchingEnemy(this);
    //         }
    //     }
    // }
}