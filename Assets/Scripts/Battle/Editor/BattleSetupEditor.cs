using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BattleSetup))]
public class BattleSetupEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        BattleSetup setup = (BattleSetup)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Auto-Find Hero and Enemy GameObjects")) {
            AutoFindGameObjects(setup);
        }
        
        if (GUILayout.Button("Auto-Load Enemy Sprites from Sprites/Enemies")) {
            LoadEnemySprites(setup);
        }
        
        EditorGUILayout.HelpBox(
            "1. Click 'Auto-Find Hero and Enemy GameObjects' to automatically find Protag_1-3 and Antag_1-3\n" +
            "2. Click 'Auto-Load Enemy Sprites' to load all enemy sprites (excluding CyberDragon)",
            MessageType.Info
        );
    }
    
    void AutoFindGameObjects(BattleSetup setup) {
        // Find hero objects
        for (int i = 0; i < 3; i++) {
            string heroName = $"Protag_{i + 1}";
            GameObject heroObj = GameObject.Find(heroName);
            if (heroObj != null) {
                setup.heroObjects[i] = heroObj;
                Debug.Log($"BattleSetupEditor: Found {heroName}");
            } else {
                Debug.LogWarning($"BattleSetupEditor: Could not find {heroName}");
            }
        }
        
        // Find enemy objects
        for (int i = 0; i < 3; i++) {
            string enemyName = $"Antag_{i + 1}";
            GameObject enemyObj = GameObject.Find(enemyName);
            if (enemyObj != null) {
                setup.enemyObjects[i] = enemyObj;
                Debug.Log($"BattleSetupEditor: Found {enemyName}");
            } else {
                Debug.LogWarning($"BattleSetupEditor: Could not find {enemyName}");
            }
        }
        
        EditorUtility.SetDirty(setup);
    }
    
    void LoadEnemySprites(BattleSetup setup) {
        // Find all sprites in the Sprites/Enemies folder
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Enemies" });
        
        if (guids.Length == 0) {
            Debug.LogWarning("No sprites found in Assets/Sprites/Enemies folder!");
            return;
        }
        
        Sprite[] sprites = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite != null && !sprite.name.Contains("CyberDragon"))
            .ToArray();
        
        setup.allEnemySprites = sprites;
        
        EditorUtility.SetDirty(setup);
        Debug.Log($"Loaded {sprites.Length} enemy sprites (excluding CyberDragon): {string.Join(", ", sprites.Select(s => s.name))}");
    }
}

