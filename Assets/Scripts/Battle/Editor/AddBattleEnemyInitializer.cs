#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to add BattleEnemyInitializer component to all Antag GameObjects
/// </summary>
public class AddBattleEnemyInitializer : EditorWindow {
    [MenuItem("Tools/Add BattleEnemyInitializer to Antags")]
    public static void AddToAllAntags() {
        // Get the current scene
        UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
        
        if (currentScene == null || !currentScene.isLoaded) {
            EditorUtility.DisplayDialog("Error", "No active scene found!", "OK");
            return;
        }
        
        int addedCount = 0;
        
        // Find all Antag GameObjects
        for (int i = 1; i <= 3; i++) {
            string antagName = $"Antag_{i}";
            GameObject antag = GameObject.Find(antagName);
            
            if (antag != null) {
                // Check if component already exists
                BattleEnemyInitializer existing = antag.GetComponent<BattleEnemyInitializer>();
                if (existing == null) {
                    // Add the component
                    BattleEnemyInitializer initializer = antag.AddComponent<BattleEnemyInitializer>();
                    addedCount++;
                    Debug.Log($"Added BattleEnemyInitializer to {antagName}");
                } else {
                    Debug.Log($"{antagName} already has BattleEnemyInitializer component");
                }
            } else {
                Debug.LogWarning($"Could not find {antagName} in scene");
            }
        }
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(currentScene);
        
        EditorUtility.DisplayDialog("Complete", 
            $"Added BattleEnemyInitializer to {addedCount} Antag(s).\n\n" +
            "The scene has been marked as dirty - remember to save!",
            "OK");
    }
}
#endif

