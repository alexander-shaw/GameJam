using UnityEngine;

public class TimeCrystal : MonoBehaviour
{
    [Header("Settings")]
    public float stabilityRestore = 20f; // Increased from 10 to 20 to balance with higher enemy damage (15-20)
    public float rotationSpeed = 90f;
    
    [Header("Audio")]
    public AudioClip pickupSound;

    void Update()
    {
        // Simple rotation animation
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Restore stability
        if (GameManager.I != null)
        {
            GameManager.I.RestoreStability(stabilityRestore);
        }
        
        // Play sound
        if (AudioManager.I != null && pickupSound != null)
        {
            AudioManager.I.PlaySFX(pickupSound);
        }
        
        Destroy(gameObject);
    }
}

