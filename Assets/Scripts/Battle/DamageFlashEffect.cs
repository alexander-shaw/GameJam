using UnityEngine;
using System.Collections;

/// <summary>
/// Makes a sprite flash red when taking damage
/// Attach this to any GameObject with a SpriteRenderer that should flash on damage
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DamageFlashEffect : MonoBehaviour {
    [Header("Flash Settings")]
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;
    public int flashCount = 2;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            originalColor = spriteRenderer.color;
        }
    }
    
    /// <summary>
    /// Trigger the flash effect
    /// </summary>
    public void Flash() {
        if (spriteRenderer == null || isFlashing) return;
        StartCoroutine(FlashCoroutine());
    }
    
    IEnumerator FlashCoroutine() {
        isFlashing = true;
        float flashTime = flashDuration / flashCount;
        
        for (int i = 0; i < flashCount; i++) {
            // Flash to red
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashTime / 2);
            
            // Return to original
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashTime / 2);
        }
        
        // Ensure we're back to original color
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
    
    /// <summary>
    /// Reset color to original (useful if something else changes it)
    /// </summary>
    public void ResetColor() {
        if (spriteRenderer != null) {
            spriteRenderer.color = originalColor;
        }
    }
}

