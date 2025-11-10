using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
    [Header("Spawn Settings")]
    [Tooltip("Number of enemies to spawn")]
    public int enemyCount = 8;
    
    [Tooltip("Minimum distance from player spawn")]
    public float minDistanceFromPlayer = 5f;
    
    [Tooltip("Maximum distance from player spawn")]
    public float maxDistanceFromPlayer = 30f;
    
    [Header("Spawn Boundaries")]
    [Tooltip("Exclude this area around player spawn (initial corridor). X and Y represent half-width and half-height")]
    public Vector2 excludeCorridorSize = new Vector2(3f, 10f);
    
    [Tooltip("Manual spawn bounds override. Leave at 0,0,0,0 to auto-detect from tilemap")]
    public Bounds manualSpawnBounds = new Bounds(Vector3.zero, Vector3.zero);
    
    [Header("Enemy Sprites")]
    [Tooltip("Enemy sprites to randomly choose from. Leave empty to auto-load from Sprites/Enemies folder")]
    public Sprite[] enemySprites;
    
    [Header("Enemy Configuration")]
    public float enemyScale = 2.9253f;
    public float enemySpeed = 3f;
    public float enemyHealth = 2f;
    public float enemyDamage = 15f;
    public float enemyChaseRange = 5f;
    
    private Transform playerTransform;
    private Tilemap tilemap;
    private Bounds tilemapBounds;
    private Vector3 playerSpawnPos;
    
    void Start() {
        // Remove enemy that was defeated in previous battle
        if (!string.IsNullOrEmpty(BattleData.enemyToRemoveName)) {
            GameObject enemyToRemove = GameObject.Find(BattleData.enemyToRemoveName);
            if (enemyToRemove != null) {
                Debug.Log($"EnemySpawner: Removing defeated enemy '{BattleData.enemyToRemoveName}' from WorldScene");
                Destroy(enemyToRemove);
            }
            BattleData.enemyToRemoveName = ""; // Clear after removal
        }
        Debug.Log("EnemySpawner: Starting...");
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            playerTransform = playerObj.transform;
            playerSpawnPos = playerTransform.position;
            Debug.Log($"EnemySpawner: Found player at {playerSpawnPos}");
        } else {
            Debug.LogWarning("EnemySpawner: Player not found! Enemies will spawn at origin.");
            playerSpawnPos = Vector3.zero;
        }
        
        // Find tilemap to determine spawn bounds
        FindTilemapBounds();
        
        // Load enemy sprites if not assigned
        if (enemySprites == null || enemySprites.Length == 0) {
            Debug.Log("EnemySpawner: No sprites assigned, attempting to load...");
            LoadEnemySprites();
        } else {
            Debug.Log($"EnemySpawner: {enemySprites.Length} sprites already assigned.");
        }
        
        // Spawn enemies
        SpawnEnemies();
    }
    
    void LoadEnemySprites() {
        // Try to load sprites from Resources/Enemies first
        Sprite[] resourcesSprites = Resources.LoadAll<Sprite>("Enemies");
        
        if (resourcesSprites != null && resourcesSprites.Length > 0) {
            enemySprites = resourcesSprites;
            Debug.Log($"Loaded {enemySprites.Length} enemy sprites from Resources/Enemies");
            return;
        }
        
        // If not in Resources, try loading from the Sprites/Enemies folder using Object.FindObjectsOfType
        // Note: This requires sprites to be in the scene or assigned manually
        // For now, we'll create a list that can be populated in the editor
        List<Sprite> spriteList = new List<Sprite>();
        
        // Try to find sprites by loading them directly (this won't work at runtime without Resources)
        // So we'll leave it empty and let the user assign them in the editor
        enemySprites = spriteList.ToArray();
        
        if (enemySprites.Length == 0) {
            Debug.LogWarning("No enemy sprites found! Please assign enemy sprites in the EnemySpawner component in the Inspector.");
        }
    }
    
    void SpawnEnemies() {
        if (enemySprites == null || enemySprites.Length == 0) {
            Debug.LogError("EnemySpawner: Cannot spawn enemies - No enemy sprites assigned! " +
                          "Please use the 'Auto-Load Enemy Sprites' button in the Inspector, or manually assign sprites.");
            return;
        }
        
        // Validate sprites array
        int validSpriteCount = 0;
        for (int i = 0; i < enemySprites.Length; i++) {
            if (enemySprites[i] != null) {
                validSpriteCount++;
                Debug.Log($"EnemySpawner: Sprite[{i}] = '{enemySprites[i].name}' (valid)");
            } else {
                Debug.LogWarning($"EnemySpawner: Sprite[{i}] is NULL!");
            }
        }
        
        if (validSpriteCount == 0) {
            Debug.LogError("EnemySpawner: All sprites in array are null! Cannot spawn enemies.");
            return;
        }
        
        Vector2 playerPos = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;
        Debug.Log($"EnemySpawner: Spawning {enemyCount} enemies around player position {playerPos} (using {validSpriteCount} valid sprites)");
        
        int spawnedCount = 0;
        for (int i = 0; i < enemyCount; i++) {
            // Generate random position
            Vector2 spawnPos = GetRandomSpawnPosition(playerPos);
            
            // Pick random VALID sprite
            Sprite randomSprite = null;
            int attempts = 0;
            while (randomSprite == null && attempts < 100) {
                int randomIndex = Random.Range(0, enemySprites.Length);
                randomSprite = enemySprites[randomIndex];
                attempts++;
            }
            
            if (randomSprite != null) {
                // Create enemy
                CreateEnemy(spawnPos, randomSprite, $"Enemy_{i + 1}");
                spawnedCount++;
            } else {
                Debug.LogError($"EnemySpawner: Could not find a valid sprite after {attempts} attempts!");
            }
        }
        
        Debug.Log($"EnemySpawner: Successfully spawned {spawnedCount} out of {enemyCount} enemies around the map");
        
        // Wait a frame and verify enemies exist in scene
        StartCoroutine(VerifyEnemiesAfterFrame(spawnedCount));
    }
    
    void FindTilemapBounds() {
        // Find all tilemaps in the scene
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        
        if (tilemaps.Length == 0) {
            Debug.LogWarning("EnemySpawner: No tilemaps found! Using manual bounds or default.");
            if (manualSpawnBounds.size != Vector3.zero) {
                tilemapBounds = manualSpawnBounds;
            } else {
                // Default bounds if nothing found
                tilemapBounds = new Bounds(Vector3.zero, new Vector3(20, 50, 0));
            }
            return;
        }
        
        // Use the largest tilemap or the one with collision
        Tilemap mainTilemap = null;
        float largestArea = 0;
        
        foreach (Tilemap tm in tilemaps) {
            Bounds bounds = tm.localBounds;
            float area = bounds.size.x * bounds.size.y;
            
            // Prefer tilemaps with colliders (walkable areas)
            if (tm.GetComponent<TilemapCollider2D>() != null) {
                mainTilemap = tm;
                tilemap = tm;
                break;
            }
            
            if (area > largestArea) {
                largestArea = area;
                mainTilemap = tm;
            }
        }
        
        if (mainTilemap == null) {
            mainTilemap = tilemaps[0];
        }
        
        tilemap = mainTilemap;
        
        // Calculate world bounds from tilemap
        Bounds localBounds = mainTilemap.localBounds;
        Vector3 worldMin = mainTilemap.transform.TransformPoint(localBounds.min);
        Vector3 worldMax = mainTilemap.transform.TransformPoint(localBounds.max);
        
        tilemapBounds = new Bounds();
        tilemapBounds.SetMinMax(worldMin, worldMax);
        
        Debug.Log($"EnemySpawner: Found tilemap bounds - Center: {tilemapBounds.center}, Size: {tilemapBounds.size}");
    }
    
    bool IsValidSpawnPosition(Vector2 position) {
        // Check if position is within tilemap bounds
        if (!tilemapBounds.Contains(new Vector3(position.x, position.y, 0))) {
            return false;
        }
        
        // Check if position is in the excluded corridor (initial player area)
        Vector2 corridorMin = new Vector2(
            playerSpawnPos.x - excludeCorridorSize.x,
            playerSpawnPos.y - excludeCorridorSize.y
        );
        Vector2 corridorMax = new Vector2(
            playerSpawnPos.x + excludeCorridorSize.x,
            playerSpawnPos.y + excludeCorridorSize.y
        );
        
        if (position.x >= corridorMin.x && position.x <= corridorMax.x &&
            position.y >= corridorMin.y && position.y <= corridorMax.y) {
            return false; // In excluded corridor
        }
        
        // Check if there's actually a tile at this position (optional - can be expensive)
        if (tilemap != null) {
            Vector3Int cellPos = tilemap.WorldToCell(new Vector3(position.x, position.y, 0));
            if (tilemap.GetTile(cellPos) == null) {
                // No tile here, might be outside walkable area
                // But we'll allow it since some areas might be intentionally empty
                // You can uncomment this to be stricter:
                // return false;
            }
        }
        
        return true;
    }
    
    Vector2 GetRandomSpawnPosition(Vector2 center) {
        int maxAttempts = 100;
        int attempts = 0;
        
        while (attempts < maxAttempts) {
            // Generate position in a circle around the player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            Vector2 candidatePos = center + offset;
            
            // Try to find a valid position within tilemap bounds
            if (IsValidSpawnPosition(candidatePos)) {
                return candidatePos;
            }
            
            attempts++;
        }
        
        // Fallback: try random positions within tilemap bounds
        for (int i = 0; i < 50; i++) {
            Vector2 randomPos = new Vector2(
                Random.Range(tilemapBounds.min.x, tilemapBounds.max.x),
                Random.Range(tilemapBounds.min.y, tilemapBounds.max.y)
            );
            
            if (IsValidSpawnPosition(randomPos)) {
                Debug.Log($"EnemySpawner: Found valid spawn position after {attempts + i} attempts: {randomPos}");
                return randomPos;
            }
        }
        
        // Last resort: spawn at center of tilemap (excluding corridor)
        Vector2 fallbackPos = new Vector2(tilemapBounds.center.x, tilemapBounds.center.y);
        Debug.LogWarning($"EnemySpawner: Could not find valid spawn position after {maxAttempts + 50} attempts. Using fallback: {fallbackPos}");
        return fallbackPos;
    }
    
    void CreateEnemy(Vector2 position, Sprite sprite, string name) {
        if (sprite == null) {
            Debug.LogError($"Cannot create enemy {name}: Sprite is null!");
            return;
        }
        
        Debug.Log($"CreateEnemy: Creating '{name}' at {position} with sprite '{sprite.name}'");
        
        // Create GameObject and ensure it's in the scene
        GameObject enemyObj = new GameObject(name);
        enemyObj.transform.position = new Vector3(position.x, position.y, 0);
        enemyObj.transform.localScale = Vector3.one * enemyScale;
        
        // Explicitly ensure it's in the active scene
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
            enemyObj, 
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );
        
        // Make sure it's active
        enemyObj.SetActive(true);
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = enemyObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.color = Color.white;
        
        // Verify sprite was assigned
        if (spriteRenderer.sprite == null) {
            Debug.LogError($"CreateEnemy: Failed to assign sprite '{sprite.name}' to SpriteRenderer! Creating fallback visual.");
            // Create a simple colored quad as fallback so enemy is visible
            CreateFallbackVisual(enemyObj);
        } else {
            Debug.Log($"CreateEnemy: SpriteRenderer has sprite '{spriteRenderer.sprite.name}'");
        }
        
        // Add Rigidbody2D (REQUIRED by EnemyAI)
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Add CapsuleCollider2D (for collision detection)
        CapsuleCollider2D collider = enemyObj.AddComponent<CapsuleCollider2D>();
        // Use sprite bounds for collider size (matching existing enemy setup)
        Bounds spriteBounds = sprite.bounds;
        collider.size = new Vector2(spriteBounds.size.x, spriteBounds.size.y);
        collider.direction = CapsuleDirection2D.Vertical;
        
        // Add EnemyAI script (REQUIRED - this is what makes them chase the player)
        EnemyAI enemyAI = enemyObj.AddComponent<EnemyAI>();
        enemyAI.speed = enemySpeed;
        enemyAI.health = enemyHealth;
        enemyAI.damage = enemyDamage;
        enemyAI.chaseRange = enemyChaseRange;
        
        // Verify all components
        if (enemyObj.GetComponent<SpriteRenderer>() == null) Debug.LogError($"CreateEnemy: {name} missing SpriteRenderer!");
        if (enemyObj.GetComponent<Rigidbody2D>() == null) Debug.LogError($"CreateEnemy: {name} missing Rigidbody2D!");
        if (enemyObj.GetComponent<CapsuleCollider2D>() == null) Debug.LogError($"CreateEnemy: {name} missing CapsuleCollider2D!");
        if (enemyObj.GetComponent<EnemyAI>() == null) Debug.LogError($"CreateEnemy: {name} missing EnemyAI!");
        
        Debug.Log($"CreateEnemy: Successfully created '{name}' at {enemyObj.transform.position}. " +
                  $"Sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}, " +
                  $"Active: {enemyObj.activeSelf}, " +
                  $"Components: SR={spriteRenderer != null}, RB={rb != null}, COL={collider != null}, AI={enemyAI != null}");
    }
    
    void CreateFallbackVisual(GameObject obj) {
        // Create a simple colored sprite as fallback
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) {
            // Create a simple white square sprite
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = Color.red; // Red square so it's obvious
            }
            texture.SetPixels(pixels);
            texture.Apply();
            Sprite fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            sr.sprite = fallbackSprite;
            Debug.Log($"CreateEnemy: Created fallback red square sprite for {obj.name}");
        }
    }
    
    IEnumerator VerifyEnemiesAfterFrame(int expectedCount) {
        yield return null; // Wait one frame
        
        // Find all GameObjects in scene and check for enemies
        int enemyObjects = 0;
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects) {
            if (obj != null && obj.name.StartsWith("Enemy_")) {
                enemyObjects++;
                Debug.Log($"EnemySpawner: Found enemy '{obj.name}' at {obj.transform.position}. " +
                         $"Active: {obj.activeSelf}, Scene: {obj.scene.name}, " +
                         $"SR: {obj.GetComponent<SpriteRenderer>() != null}, " +
                         $"Destroyed: {obj == null}");
            }
        }
        
        Debug.Log($"EnemySpawner: Verification - Expected {expectedCount} enemies, Found {enemyObjects} in scene. " +
                 $"Total objects in scene: {allObjects.Length}");
        
        if (enemyObjects == 0) {
            Debug.LogError("EnemySpawner: CRITICAL - No enemies found in scene even though creation succeeded! " +
                          "This suggests enemies are being destroyed or not added to scene hierarchy.");
        }
    }
}

