using UnityEngine;
using BattleSystem;

/// <summary>
/// Sets up the boss battle scene with a massive CyberDragon
/// </summary>
public class BossBattleSetup : MonoBehaviour {
    [Header("Hero GameObjects")]
    public GameObject[] heroObjects = new GameObject[3]; // Protag_1, Protag_2, Protag_3
    
    [Header("Boss GameObject")]
    public GameObject bossObject; // The massive CyberDragon (should be Antag_1 or similar)
    
    [Header("Boss Sprite")]
    public Sprite cyberDragonSprite;
    
    private BattleManager battleManager;
    
    void Start() {
        // Auto-find GameObjects if not assigned
        AutoFindGameObjects();
        
        // Find BattleManager
        battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager == null) {
            Debug.LogError("BossBattleSetup: BattleManager not found!");
            return;
        }
        
        // Load CyberDragon sprite if not assigned
        if (cyberDragonSprite == null) {
            LoadCyberDragonSprite();
        }
        
        // Setup boss battle
        SetupBossBattle();
    }
    
    void AutoFindGameObjects() {
        // Auto-find hero objects if not assigned
        for (int i = 0; i < heroObjects.Length; i++) {
            if (heroObjects[i] == null) {
                string heroName = $"Protag_{i + 1}";
                GameObject heroObj = GameObject.Find(heroName);
                if (heroObj != null) {
                    heroObjects[i] = heroObj;
                    Debug.Log($"BossBattleSetup: Auto-found {heroName}");
                }
            }
        }
        
        // Auto-find boss object if not assigned
        if (bossObject == null) {
            // Try common names
            GameObject boss = GameObject.Find("Antag_1") ?? 
                            GameObject.Find("Boss") ?? 
                            GameObject.Find("CyberDragon");
            if (boss != null) {
                bossObject = boss;
                Debug.Log($"BossBattleSetup: Auto-found boss object '{boss.name}'");
            }
        }
    }
    
    void LoadCyberDragonSprite() {
        // Try to load from Resources
        Sprite sprite = Resources.Load<Sprite>("Enemies/CyberDragon");
        if (sprite != null) {
            cyberDragonSprite = sprite;
            Debug.Log("BossBattleSetup: Loaded CyberDragon sprite from Resources");
            return;
        }
        
        // Try to find in scene
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer sr in renderers) {
            if (sr.sprite != null && sr.sprite.name.Contains("CyberDragon")) {
                cyberDragonSprite = sr.sprite;
                Debug.Log($"BossBattleSetup: Found CyberDragon sprite in scene: {sr.sprite.name}");
                return;
            }
        }
        
        Debug.LogWarning("BossBattleSetup: Could not find CyberDragon sprite! Please assign in Inspector.");
    }
    
    void SetupBossBattle() {
        // Apply CyberDragon sprite to boss object
        if (bossObject != null && cyberDragonSprite != null) {
            SpriteRenderer sr = bossObject.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.sprite = cyberDragonSprite;
                Debug.Log($"BossBattleSetup: Applied CyberDragon sprite to boss object");
            }
            
            // Make the boss massive (scale it up)
            bossObject.transform.localScale = new Vector3(10f, 10f, 10f); // Much larger than normal enemies
            Debug.Log("BossBattleSetup: Scaled boss to massive size");
        }
        
        // Hide or disable other enemy objects (Antag_2, Antag_3)
        for (int i = 2; i <= 3; i++) {
            GameObject enemyObj = GameObject.Find($"Antag_{i}");
            if (enemyObj != null) {
                enemyObj.SetActive(false);
                Debug.Log($"BossBattleSetup: Hid Antag_{i}");
            }
        }
        
        // Setup battle system with Dragon enemy
        SetupBattleSystem();
        
        // Clear battle data
        BattleData.Clear();
    }
    
    void SetupBattleSystem() {
        if (battleManager == null) return;
        
        // Clear existing enemies
        battleManager.enemyGroup.enemies.Clear();
        
        // Create the Dragon boss enemy
        Dragon dragon = new Dragon();
        battleManager.enemyGroup.AddEnemy(dragon);
        Debug.Log("BossBattleSetup: Added Dragon boss to battle system");
        
        // Setup heroes (default party)
        battleManager.party.heroes.Clear();
        battleManager.party.AddHero(new BlinkerBell());
        battleManager.party.AddHero(new Blandroid());
        battleManager.party.AddHero(new Jachariah());
        
        // Initialize the battle
        battleManager.InitializeBattle();
    }
}

