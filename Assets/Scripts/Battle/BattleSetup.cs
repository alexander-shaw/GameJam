using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using BattleSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Sets up the battle scene with the correct enemies based on what touched the player
/// </summary>
public class BattleSetup : MonoBehaviour {
    [Header("Hero GameObjects")]
    public GameObject[] heroObjects = new GameObject[3]; // Protag_1, Protag_2, Protag_3
    
    [Header("Enemy GameObjects")]
    public GameObject[] enemyObjects = new GameObject[3]; // Antag_1, Antag_2, Antag_3
    
    [Header("Enemy Sprites")]
    [Tooltip("All available enemy sprites (will exclude CyberDragon)")]
    public Sprite[] allEnemySprites;
    
    private BattleManager battleManager;
    
    void Start() {
        // Auto-find GameObjects if not assigned
        AutoFindGameObjects();
        
        // Find BattleManager
        battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager == null) {
            Debug.LogError("BattleSetup: BattleManager not found!");
            return;
        }
        
        // Setup UI if it doesn't exist
        SetupBattleUI();
        
        // Load enemy sprites if not assigned
        if (allEnemySprites == null || allEnemySprites.Length == 0) {
            LoadEnemySprites();
        }
        
        // Setup battle
        SetupBattle();
    }
    
    void SetupBattleUI() {
        // Check if UI already exists
        if (battleManager.actionSelectionPanel != null) {
            Debug.Log("BattleSetup: UI already exists, skipping setup");
            return;
        }
        
        // Create UI directly (don't wait for Start())
        BattleUISetup.CreateUIForBattleManager(battleManager);
    }
    
    void AutoFindGameObjects() {
        // Auto-find hero objects if not assigned
        for (int i = 0; i < heroObjects.Length; i++) {
            if (heroObjects[i] == null) {
                string heroName = $"Protag_{i + 1}";
                GameObject heroObj = GameObject.Find(heroName);
                if (heroObj != null) {
                    heroObjects[i] = heroObj;
                    Debug.Log($"BattleSetup: Auto-found {heroName}");
                }
            }
        }
        
        // Auto-find enemy objects if not assigned
        for (int i = 0; i < enemyObjects.Length; i++) {
            if (enemyObjects[i] == null) {
                string enemyName = $"Antag_{i + 1}";
                GameObject enemyObj = GameObject.Find(enemyName);
                if (enemyObj != null) {
                    enemyObjects[i] = enemyObj;
                    Debug.Log($"BattleSetup: Auto-found {enemyName}");
                }
            }
        }
    }
    
    void LoadEnemySprites() {
        List<Sprite> loadedSprites = new List<Sprite>();
        
        // Try to load from Resources first
        Sprite[] resourcesSprites = Resources.LoadAll<Sprite>("Enemies");
        if (resourcesSprites != null && resourcesSprites.Length > 0) {
            foreach (Sprite sprite in resourcesSprites) {
                if (sprite != null) {
                    loadedSprites.Add(sprite);
                }
            }
        }
        
        // If no sprites in Resources, try to find them via AssetDatabase (Editor only)
        #if UNITY_EDITOR
        if (loadedSprites.Count == 0) {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Enemies" });
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) {
                    loadedSprites.Add(sprite);
                }
            }
        }
        #endif
        
        if (loadedSprites.Count > 0) {
            allEnemySprites = loadedSprites.ToArray();
            Debug.Log($"BattleSetup: Loaded {allEnemySprites.Length} enemy sprites");
        } else {
            Debug.LogWarning("BattleSetup: No enemy sprites found. Please ensure sprites are in Assets/Sprites/Enemies or Assets/Resources/Enemies");
        }
    }
    
    void SetupBattle() {
        // Get available enemy sprites (excluding CyberDragon)
        List<Sprite> availableSprites = new List<Sprite>();
        if (allEnemySprites != null) {
            foreach (Sprite sprite in allEnemySprites) {
                if (sprite != null && !sprite.name.Contains("CyberDragon")) {
                    availableSprites.Add(sprite);
                }
            }
        }
        
        if (availableSprites.Count == 0) {
            Debug.LogError("BattleSetup: No available enemy sprites (excluding CyberDragon)!");
            return;
        }
        
        // Randomly select 3 enemies (excluding CyberDragon)
        List<Sprite> enemySpritesToUse = new List<Sprite>();
        List<Sprite> remainingSprites = new List<Sprite>(availableSprites);
        
        // Select 3 random enemies
        int enemiesToSelect = Mathf.Min(3, remainingSprites.Count);
        for (int i = 0; i < enemiesToSelect; i++) {
            int randomIndex = Random.Range(0, remainingSprites.Count);
            enemySpritesToUse.Add(remainingSprites[randomIndex]);
            remainingSprites.RemoveAt(randomIndex);
        }
        
        Debug.Log($"BattleSetup: Randomly selected {enemySpritesToUse.Count} enemies: {string.Join(", ", enemySpritesToUse.ConvertAll(s => s.name))}");
        
        // Apply sprites to enemy GameObjects (Antag_1, Antag_2, Antag_3)
        for (int i = 0; i < enemyObjects.Length && i < enemySpritesToUse.Count; i++) {
            if (enemyObjects[i] != null) {
                SpriteRenderer sr = enemyObjects[i].GetComponent<SpriteRenderer>();
                if (sr != null) {
                    sr.sprite = enemySpritesToUse[i];
                    Debug.Log($"BattleSetup: Set Antag_{i + 1} sprite to '{enemySpritesToUse[i].name}'");
                }
            }
        }
        
        // Create battle system enemies based on sprites with correct stats from C++ code
        SetupBattleSystemEnemies(enemySpritesToUse);
        
        // Clear battle data for next time
        BattleData.Clear();
    }
    
    void SetupBattleSystemEnemies(List<Sprite> enemySprites) {
        if (battleManager == null) return;
        
        // Clear existing enemies
        battleManager.enemyGroup.enemies.Clear();
        
        // Create enemies based on sprite names
        foreach (Sprite sprite in enemySprites) {
            Enemy enemy = CreateEnemyFromSprite(sprite);
            if (enemy != null) {
                battleManager.enemyGroup.AddEnemy(enemy);
                Debug.Log($"BattleSetup: Added enemy '{enemy.GetName()}' to battle system");
            }
        }
        
        // Setup heroes (default party)
        battleManager.party.heroes.Clear();
        battleManager.party.AddHero(new BlinkerBell());
        battleManager.party.AddHero(new Blandroid());
        battleManager.party.AddHero(new Jachariah());
        
        // Initialize the battle
        battleManager.InitializeBattle();
    }
    
    Enemy CreateEnemyFromSprite(Sprite sprite) {
        if (sprite == null) return null;
        
        string spriteName = sprite.name.ToLower();
        
        // Map sprite names to enemy types based on actual sprite names in Assets/Sprites/Enemies
        // Matching sprite names to C++ enemy classes with correct stats:
        // - MindGoblin -> Goblin (6 HP, 3 ATK, 1 DEF, 3 Energy)
        // - Neo-Knight -> Orc (12 HP, 5 ATK, 3 DEF, 3 Energy)
        // - Ripper -> Ghoul (14 HP, 6 ATK, 1 DEF, 3 Energy)
        // - WhatTheFuck -> Warlock (8 HP, 7 ATK, 1 DEF, 4 Energy)
        // - CyberDragon -> Dragon (excluded from regular battles)
        
        if (spriteName.Contains("mindgoblin") || spriteName.Contains("goblin")) {
            // Goblin: 6 HP, 3 ATK, 1 DEF, 3 Energy (from C++ code)
            return new Goblin();
        } else if (spriteName.Contains("neo-knight") || spriteName.Contains("neoknight") || spriteName.Contains("knight")) {
            // Orc: 12 HP, 5 ATK, 3 DEF, 3 Energy (from C++ code)
            return new Orc();
        } else if (spriteName.Contains("ripper")) {
            // Ghoul: 14 HP, 6 ATK, 1 DEF, 3 Energy (from C++ code)
            return new Ghoul();
        } else if (spriteName.Contains("whatthefuck") || spriteName.Contains("warlock")) {
            // Warlock: 8 HP, 7 ATK, 1 DEF, 4 Energy (from C++ code)
            return new Warlock();
        } else {
            // Default to Goblin if sprite name doesn't match
            Debug.LogWarning($"BattleSetup: Unknown sprite name '{sprite.name}', defaulting to Goblin");
            return new Goblin();
        }
    }
}

