using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public int maxEnemies = 20;
    public float spawnDistance = 8f; // Reduced from 15 to 8 to spawn closer to camera
    
    [Header("Difficulty")]
    public float difficultyIncreaseRate = 0.05f; // How much spawn rate decreases per kill
    public float minSpawnRate = 0.5f;
    
    float spawnTimer;
    float currentSpawnRate;
    Camera mainCamera;
    int totalEnemiesSpawned = 0; // Track total spawns for difficulty scaling

    void Start()
    {
        currentSpawnRate = spawnRate;
        mainCamera = Camera.main;
        
        // Debug: Check if prefab is assigned
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Enemy Prefab is not assigned! Enemies will not spawn.");
        }
        else
        {
            Debug.Log($"EnemySpawner: Ready to spawn enemies. Prefab: {enemyPrefab.name}");
        }
        
        // Initialize spawn timer
        spawnTimer = currentSpawnRate;
    }

    void Update()
    {
        // CRITICAL: Check if prefab is assigned (only log once per second to avoid spam)
        if (enemyPrefab == null)
        {
            if (Time.time % 1f < Time.deltaTime) // Log roughly once per second
            {
                Debug.LogError("EnemySpawner: Enemy Prefab is NOT assigned! Cannot spawn enemies. " +
                    "Please assign Enemy prefab in Inspector: Select EnemySpawner → EnemySpawner component → Enemy Prefab field");
            }
            return; // Don't try to spawn if prefab is null
        }
        
        // Increase difficulty based on total enemies killed (not time)
        // More enemies killed = faster spawn rate
        if (GameManager.I != null)
        {
            int kills = GameManager.I.EnemiesKilled;
            currentSpawnRate = Mathf.Max(minSpawnRate, spawnRate - (kills * difficultyIncreaseRate));
        }
        else
        {
            currentSpawnRate = spawnRate;
        }
        
        // Count current enemies
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        
        // Spawn if timer expired and under max enemies
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && enemyCount < maxEnemies && enemyPrefab != null)
        {
            SpawnEnemy();
            spawnTimer = currentSpawnRate;
            totalEnemiesSpawned++;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Cannot spawn enemy - prefab is null!");
            return;
        }
        
        // Spawn at random edge position
        Vector2 spawnPos = GetRandomEdgePosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"EnemySpawner: Spawned enemy at {spawnPos}");
    }

    Vector2 GetRandomEdgePosition()
    {
        // Get camera bounds or use fixed arena size
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // Spawn at edge distance from center (0,0) or camera center
        Vector2 center = mainCamera != null ? (Vector2)mainCamera.transform.position : Vector2.zero;
        return center + direction * spawnDistance;
    }
}

