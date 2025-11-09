using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [Header("Background Settings")]
    public GameObject backgroundPrefab;
    public float backgroundSize = 50f;
    public int gridSize = 5; // 5x5 grid of background tiles
    
    [Header("Procedural Generation (Scaffolded)")]
    public bool useProceduralGeneration = false;
    public int seed = 0;
    
    void Start()
    {
        GenerateBackground();
    }
    
    void GenerateBackground()
    {
        if (backgroundPrefab == null)
        {
            // Create default black square background
            CreateDefaultBackground();
            return;
        }
        
        // TODO: Implement procedural generation
        // For now, just create a simple grid
        CreateSimpleGrid();
    }
    
    void CreateDefaultBackground()
    {
        // Create a large black square as background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(transform);
        bg.transform.position = Vector3.zero;
        
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.color = Color.black;
        
        // Create a large black texture
        Texture2D texture = new Texture2D(512, 512);
        Color[] pixels = new Color[512 * 512];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f), 100f);
        sr.sprite = sprite;
        sr.sortingOrder = -10; // Behind everything
        
        // Scale to cover a large area
        bg.transform.localScale = new Vector3(backgroundSize, backgroundSize, 1f);
    }
    
    void CreateSimpleGrid()
    {
        // Scaffolded: This will be expanded for procedural generation
        // For now, just create the default background
        CreateDefaultBackground();
        
        // TODO: Implement procedural tile generation
        // - Generate tiles based on seed
        // - Create varied patterns
        // - Add different tile types
        // - Implement noise-based generation
    }
    
    // Scaffolded methods for future procedural generation
    void GenerateProceduralTiles()
    {
        // TODO: Implement procedural tile generation
        // This will use Perlin noise or similar to create varied backgrounds
    }
    
    void ApplySeed(int newSeed)
    {
        seed = newSeed;
        Random.InitState(seed);
        // TODO: Regenerate background with new seed
    }
}

