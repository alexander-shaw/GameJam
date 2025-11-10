#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to add DamageFlashEffect and BattleEntityLinker to all Protag and Antag GameObjects
/// </summary>
public class AddDamageFlashComponents : EditorWindow {
    [MenuItem("Tools/Add Damage Flash Components to Battle Entities")]
    public static void AddToAllEntities() {
        UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
        
        if (currentScene == null || !currentScene.isLoaded) {
            EditorUtility.DisplayDialog("Error", "No active scene found!", "OK");
            return;
        }
        
        int addedCount = 0;
        
        // Add to all Protags
        for (int i = 1; i <= 3; i++) {
            string protagName = $"Protag_{i}";
            GameObject protag = GameObject.Find(protagName);
            
            if (protag != null) {
                // Add DamageFlashEffect if missing
                if (protag.GetComponent<DamageFlashEffect>() == null) {
                    protag.AddComponent<DamageFlashEffect>();
                    addedCount++;
                }
                
                // Add BattleEntityLinker if missing
                if (protag.GetComponent<BattleEntityLinker>() == null) {
                    BattleEntityLinker linker = protag.AddComponent<BattleEntityLinker>();
                    linker.isHero = true;
                    linker.entityIndex = i - 1;
                    addedCount++;
                }
                
                Debug.Log($"Added components to {protagName}");
            }
        }
        
        // Add to all Antags
        for (int i = 1; i <= 3; i++) {
            string antagName = $"Antag_{i}";
            GameObject antag = GameObject.Find(antagName);
            
            if (antag != null) {
                // Add DamageFlashEffect if missing
                if (antag.GetComponent<DamageFlashEffect>() == null) {
                    antag.AddComponent<DamageFlashEffect>();
                    addedCount++;
                }
                
                // Add BattleEntityLinker if missing
                if (antag.GetComponent<BattleEntityLinker>() == null) {
                    BattleEntityLinker linker = antag.AddComponent<BattleEntityLinker>();
                    linker.isHero = false;
                    linker.entityIndex = i - 1;
                    addedCount++;
                }
                
                Debug.Log($"Added components to {antagName}");
            }
        }
        
        EditorSceneManager.MarkSceneDirty(currentScene);
        
        EditorUtility.DisplayDialog("Complete", 
            $"Added {addedCount} component(s) to battle entities.\n\n" +
            "The scene has been marked as dirty - remember to save!",
            "OK");
    }
}
#endif

