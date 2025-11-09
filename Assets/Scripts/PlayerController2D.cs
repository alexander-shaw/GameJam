using UnityEngine;

public class PlayerController2D : MonoBehaviour {
    // Public variables
    public float speed = 5f; // The speed at which the player moves

    // Private variables
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for changing sprites.
    private Vector2 movement; // Stores the direction of player movement

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput);
    }

    void FixedUpdate() {
        // Apply movement to the player in FixedUpdate for physics consistency
        rb.linearVelocity = movement * speed;
        
        switch (Input.GetAxisRaw("Horizontal")) {
            case > 0:
                spriteRenderer.flipX = false;
                break;
            case < 0:
                spriteRenderer.flipX = true;
                break;
            case 0:
                // Forward-facing sprite.
                break;
        }
    }
}