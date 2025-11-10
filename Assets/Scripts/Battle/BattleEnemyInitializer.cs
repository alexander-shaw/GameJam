using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Initializes battle enemy stats based on sprite name, matching C++ code values
/// Also randomly selects and assigns enemy sprites to Antag GameObjects
/// Attach this to Antag GameObjects in the battle scene
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BattleEnemyInitializer : MonoBehaviour {
    [Header("Auto-Initialized")]
    [Tooltip("Enemy type determined from sprite name")]
    public string enemyType = "Unknown";
    
    private static bool spritesInitialized = false;
    private static List<Sprite> availableEnemySprites = new List<Sprite>();
    private static List<Sprite> usedSprites = new List<Sprite>(); // Track which sprites have been assigned
    
    private BattleEntity battleEntity;
    private SpriteRenderer spriteRenderer;
    private int antagIndex = -1; // Which Antag this is (1, 2, or 3)
    
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        battleEntity = GetComponent<BattleEntity>();
        
        if (spriteRenderer == null) {
            Debug.LogWarning($"BattleEnemyInitializer: No SpriteRenderer found on {gameObject.name}");
            return;
        }
        
        if (battleEntity == null) {
            Debug.LogWarning($"BattleEnemyInitializer: No BattleEntity component found on {gameObject.name}");
            return;
        }
        
        // Determine which Antag this is
        DetermineAntagIndex();
        
        // Reset static state for new battle (Antag_1 runs first)
        if (antagIndex == 1) {
            usedSprites.Clear();
        }
        
        // Load available enemy sprites if not already loaded
        if (!spritesInitialized) {
            LoadAvailableEnemySprites();
        }
        
        // Select and assign sprite for this Antag
        SelectAndAssignSprite();
        
        // Initialize stats based on the assigned sprite
        InitializeEnemyStats();
    }
    
    void DetermineAntagIndex() {
        string name = gameObject.name;
        if (name.Contains("Antag_1")) antagIndex = 1;
        else if (name.Contains("Antag_2")) antagIndex = 2;
        else if (name.Contains("Antag_3")) antagIndex = 3;
    }
    
    void LoadAvailableEnemySprites() {
        availableEnemySprites.Clear();
        
        // Try to load from Resources first
        Sprite[] resourcesSprites = Resources.LoadAll<Sprite>("Enemies");
        if (resourcesSprites != null && resourcesSprites.Length > 0) {
            foreach (Sprite sprite in resourcesSprites) {
                if (sprite != null && !sprite.name.Contains("CyberDragon")) {
                    availableEnemySprites.Add(sprite);
                }
            }
        }
        
        // If no sprites in Resources, try to find them via AssetDatabase (Editor only)
        #if UNITY_EDITOR
        if (availableEnemySprites.Count == 0) {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Enemies" });
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null && !sprite.name.Contains("CyberDragon")) {
                    availableEnemySprites.Add(sprite);
                }
            }
        }
        #endif
        
        if (availableEnemySprites.Count == 0) {
            Debug.LogError("BattleEnemyInitializer: No enemy sprites found in Assets/Sprites/Enemies (excluding CyberDragon)!");
            return;
        }
        
        spritesInitialized = true;
        Debug.Log($"BattleEnemyInitializer: Loaded {availableEnemySprites.Count} available enemy sprites");
    }
    
    void SelectAndAssignSprite() {
        if (availableEnemySprites.Count == 0) {
            Debug.LogWarning($"BattleEnemyInitializer: No sprites available for {gameObject.name}");
            return;
        }
        
        Sprite selectedSprite = null;
        
        // Antag_1 gets the primary enemy from BattleData (the one that touched the player)
        if (antagIndex == 1) {
            Sprite primarySprite = BattleData.primaryEnemySprite;
            if (primarySprite != null && !primarySprite.name.Contains("CyberDragon")) {
                selectedSprite = primarySprite;
                usedSprites.Add(selectedSprite);
                Debug.Log($"BattleEnemyInitializer: Antag_1 using primary enemy sprite '{primarySprite.name}'");
            } else {
                // Fallback to random if no primary sprite or it's CyberDragon
                List<Sprite> remaining = availableEnemySprites.Where(s => !usedSprites.Contains(s)).ToList();
                if (remaining.Count > 0) {
                    selectedSprite = remaining[Random.Range(0, remaining.Count)];
                } else {
                    selectedSprite = availableEnemySprites[Random.Range(0, availableEnemySprites.Count)];
                }
                usedSprites.Add(selectedSprite);
                Debug.Log($"BattleEnemyInitializer: Antag_1 using random sprite '{selectedSprite.name}' (no valid primary sprite)");
            }
        } else {
            // Antag_2 and Antag_3 get random sprites (excluding CyberDragon and already used sprites)
            List<Sprite> remainingSprites = availableEnemySprites.Where(s => !usedSprites.Contains(s)).ToList();
            
            if (remainingSprites.Count > 0) {
                selectedSprite = remainingSprites[Random.Range(0, remainingSprites.Count)];
                usedSprites.Add(selectedSprite);
                Debug.Log($"BattleEnemyInitializer: Antag_{antagIndex} using random sprite '{selectedSprite.name}'");
            } else {
                // Fallback if we've run out of unique sprites (shouldn't happen with 4+ sprites)
                selectedSprite = availableEnemySprites[Random.Range(0, availableEnemySprites.Count)];
                Debug.LogWarning($"BattleEnemyInitializer: Antag_{antagIndex} using fallback sprite '{selectedSprite.name}' (no unique sprites left)");
            }
        }
        
        // Apply the sprite
        if (selectedSprite != null && spriteRenderer != null) {
            spriteRenderer.sprite = selectedSprite;
        }
    }
    
    void InitializeEnemyStats() {
        if (spriteRenderer == null || spriteRenderer.sprite == null) {
            Debug.LogWarning($"BattleEnemyInitializer: No sprite to initialize stats for {gameObject.name}");
            return;
        }
        
        string spriteName = spriteRenderer.sprite.name.ToLower();
        
        // Map sprite names to enemy types with stats from C++ code:
        // - MindGoblin -> Goblin (6 HP, 3 ATK, 1 DEF, 3 Energy)
        // - Neo-Knight -> Orc (12 HP, 5 ATK, 3 DEF, 3 Energy)
        // - Ripper -> Ghoul (14 HP, 6 ATK, 1 DEF, 3 Energy)
        // - WhatTheFuck -> Warlock (8 HP, 7 ATK, 1 DEF, 4 Energy)
        // - CyberDragon -> Dragon (40 HP, 8 ATK, 4 DEF, 0 Energy) - should not appear in regular battles
        
        if (spriteName.Contains("mindgoblin") || spriteName.Contains("goblin")) {
            // Goblin: 6 HP, 3 ATK, 1 DEF, 3 Energy (from C++ code)
            enemyType = "Goblin";
            battleEntity.health = 6f;
            battleEntity.attackPower = 3f;
            battleEntity.defense = 1f;
            battleEntity.energy = 3f;
            battleEntity.attack1Name = "Trick";
            battleEntity.attack1Damage = 0f; // Trick doesn't deal damage, it modifies stats
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 3f;
        }
        else if (spriteName.Contains("neo-knight") || spriteName.Contains("neoknight") || spriteName.Contains("knight")) {
            // Orc: 12 HP, 5 ATK, 3 DEF, 3 Energy (from C++ code)
            enemyType = "Orc";
            battleEntity.health = 12f;
            battleEntity.attackPower = 5f;
            battleEntity.defense = 3f;
            battleEntity.energy = 3f;
            battleEntity.attack1Name = "Brutalize";
            battleEntity.attack1Damage = 5f; // Brutalize deals attack damage + reduces defense
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 5f;
        }
        else if (spriteName.Contains("ripper")) {
            // Ghoul: 14 HP, 6 ATK, 1 DEF, 3 Energy (from C++ code)
            enemyType = "Ghoul";
            battleEntity.health = 14f;
            battleEntity.attackPower = 6f;
            battleEntity.defense = 1f;
            battleEntity.energy = 3f;
            battleEntity.attack1Name = "Drain Bite";
            battleEntity.attack1Damage = 6f; // Drain Bite deals damage and heals
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 6f;
        }
        else if (spriteName.Contains("whatthefuck") || spriteName.Contains("warlock")) {
            // Warlock: 8 HP, 7 ATK, 1 DEF, 4 Energy (from C++ code)
            enemyType = "Warlock";
            battleEntity.health = 8f;
            battleEntity.attackPower = 7f;
            battleEntity.defense = 1f;
            battleEntity.energy = 4f;
            battleEntity.attack1Name = "Burst";
            battleEntity.attack1Damage = 14f; // Burst deals 2x attack damage
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 7f;
        }
        else if (spriteName.Contains("cyberdragon") || spriteName.Contains("cyber-dragon") || spriteName.Contains("dragon")) {
            // Dragon: 40 HP, 8 ATK, 4 DEF, 0 Energy (from C++ code)
            enemyType = "Dragon";
            battleEntity.health = 40f;
            battleEntity.attackPower = 8f;
            battleEntity.defense = 4f;
            battleEntity.energy = 0f;
            battleEntity.attack1Name = "Dragon Ability";
            battleEntity.attack1Damage = 8f; // Various dragon abilities
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 8f;
        }
        else {
            // Default to Goblin if sprite name doesn't match
            Debug.LogWarning($"BattleEnemyInitializer: Unknown sprite name '{spriteRenderer.sprite.name}' on {gameObject.name}, defaulting to Goblin");
            enemyType = "Goblin";
            battleEntity.health = 6f;
            battleEntity.attackPower = 3f;
            battleEntity.defense = 1f;
            battleEntity.energy = 3f;
            battleEntity.attack1Name = "Trick";
            battleEntity.attack1Damage = 0f;
            battleEntity.attack2Name = "Attack";
            battleEntity.attack2Damage = 3f;
        }
        
        Debug.Log($"BattleEnemyInitializer: Initialized {gameObject.name} as {enemyType} (HP: {battleEntity.health}, ATK: {battleEntity.attackPower}, DEF: {battleEntity.defense}, EP: {battleEntity.energy})");
    }
    
    /// <summary>
    /// Gets the enemy type name for display purposes
    /// </summary>
    public string GetEnemyType() {
        return enemyType;
    }
}
