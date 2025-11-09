using UnityEngine;

public class PlayerController2D : MonoBehaviour {
    // Public variables
    public float speed = 5f; // The speed at which the player moves
    public bool canMoveDiagonally = true; // Controls whether the player can move diagonally
    public float rotationSpeed = 720f; // Degrees per second for smooth rotation

    // Private variables
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private Vector2 movement; // Stores the direction of player movement
    private bool isMovingHorizontally = true; // Flag to track if the player is moving horizontally
    private Quaternion targetRotation; // Desired rotation to smoothly interpolate to

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        targetRotation = transform.rotation;
    }

    void Update() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (canMoveDiagonally) {
            movement = new Vector2(horizontalInput, verticalInput);
            UpdateTargetRotation(horizontalInput, verticalInput);
        } else {
            if (horizontalInput != 0) {
                isMovingHorizontally = true;
            } else if (verticalInput != 0) {
                isMovingHorizontally = false;
            }

            if (isMovingHorizontally) {
                movement = new Vector2(horizontalInput, 0);
                UpdateTargetRotation(horizontalInput, 0);
            } else {
                movement = new Vector2(0, verticalInput);
                UpdateTargetRotation(0, verticalInput);
            }
        }

        // Smoothly rotate towards the target rotation every frame
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void FixedUpdate() {
        // Apply movement to the player in FixedUpdate for physics consistency
        rb.linearVelocity = movement * speed;
    }

    void UpdateTargetRotation(float x, float y) {
        if (x == 0 && y == 0) return; // keep current target when no input
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler(0, 0, angle);
    }
}