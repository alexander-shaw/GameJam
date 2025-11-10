using UnityEngine;

public class PlayerController2D : MonoBehaviour {
    // Public variables
    public float speed = 5f; // The speed at which the player moves

    // Private variables
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for changing sprites.
    private Vector2 movement; // Stores the direction of player movement
    private AudioSource audioSource; // Reference to the AudioSource component for footsteps
    private AudioClip footstepsClip; // The footsteps audio clip

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Setup audio for footsteps
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load footsteps audio clip from Resources/Audio folder
        footstepsClip = Resources.Load<AudioClip>("Audio/footsteps");
        
        if (footstepsClip != null) {
            audioSource.clip = footstepsClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        } else {
            Debug.LogWarning("PlayerController2D: Could not load footsteps audio clip!");
        }
    }

    void Update() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput);
    }

    void FixedUpdate() {
        // Apply movement to the player in FixedUpdate for physics consistency
        rb.linearVelocity = movement * speed;
        
        // Handle footsteps audio
        bool isMoving = movement.magnitude > 0.1f;
        if (audioSource != null && footstepsClip != null) {
            if (isMoving && !audioSource.isPlaying) {
                audioSource.Play();
            } else if (!isMoving && audioSource.isPlaying) {
                audioSource.Stop();
            }
        }
        
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