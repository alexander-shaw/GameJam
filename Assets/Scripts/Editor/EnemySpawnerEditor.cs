using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        EnemySpawner spawner = (EnemySpawner)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemy Sprite Loading", EditorStyles.boldLabel);
        
        // Show current sprite count
        int spriteCount = spawner.enemySprites != null ? spawner.enemySprites.Length : 0;
        EditorGUILayout.LabelField($"Currently loaded: {spriteCount} sprites");
        
        if (spriteCount > 0) {
            EditorGUILayout.HelpBox(
                $"Sprites loaded: {string.Join(", ", spawner.enemySprites.Select(s => s != null ? s.name : "null"))}",
                MessageType.Info
            );
        }
        
        if (GUILayout.Button("Auto-Load Enemy Sprites from Sprites/Enemies", GUILayout.Height(30))) {
            LoadEnemySprites(spawner);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "IMPORTANT: You must click the button above to load enemy sprites before playing! " +
            "The sprites need to be assigned in the Inspector for them to work at runtime. " +
            "After clicking, you should see the sprites appear in the 'Enemy Sprites' array above.",
            spriteCount == 0 ? MessageType.Warning : MessageType.Info
        );
    }
    
    void LoadEnemySprites(EnemySpawner spawner) {
        // Find all sprites in the Sprites/Enemies folder
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Enemies" });
        
        if (guids.Length == 0) {
            Debug.LogWarning("No sprites found in Assets/Sprites/Enemies folder!");
            return;
        }
        
        Sprite[] sprites = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(sprite => sprite != null)
            .ToArray();
        
        spawner.enemySprites = sprites;
        
        EditorUtility.SetDirty(spawner);
        Debug.Log($"Loaded {sprites.Length} enemy sprites: {string.Join(", ", sprites.Select(s => s.name))}");
    }
}

