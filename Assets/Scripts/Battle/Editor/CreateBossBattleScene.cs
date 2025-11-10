#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor script to create and set up the BossBattleScene
/// </summary>
public class CreateBossBattleScene : EditorWindow {
    [MenuItem("Tools/Create Boss Battle Scene")]
    public static void ShowWindow() {
        GetWindow<CreateBossBattleScene>("Create Boss Battle Scene");
    }
    
    void OnGUI() {
        GUILayout.Label("Boss Battle Scene Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create BossBattleScene from BattleScene")) {
            CreateBossScene();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will:\n" +
            "1. Duplicate BattleScene as BossBattleScene\n" +
            "2. Hide Antag_2 and Antag_3\n" +
            "3. Scale Antag_1 to massive size (10x)\n" +
            "4. Add BossBattleSetup component",
            MessageType.Info
        );
    }
    
    static void CreateBossScene() {
        // Check if BattleScene exists
        string battleScenePath = "Assets/Scenes/BattleScene.unity";
        string bossScenePath = "Assets/Scenes/BossBattleScene.unity";
        
        if (!System.IO.File.Exists(battleScenePath)) {
            EditorUtility.DisplayDialog("Error", "BattleScene.unity not found!", "OK");
            return;
        }
        
        // Copy BattleScene to BossBattleScene
        if (System.IO.File.Exists(bossScenePath)) {
            if (!EditorUtility.DisplayDialog("Overwrite?", 
                "BossBattleScene.unity already exists. Overwrite?", "Yes", "No")) {
                return;
            }
        }
        
        System.IO.File.Copy(battleScenePath, bossScenePath, true);
        AssetDatabase.Refresh();
        
        // Load the new scene
        Scene bossScene = EditorSceneManager.OpenScene(bossScenePath);
        
        // Find and modify GameObjects
        GameObject antag1 = GameObject.Find("Antag_1");
        GameObject antag2 = GameObject.Find("Antag_2");
        GameObject antag3 = GameObject.Find("Antag_3");
        
        if (antag1 != null) {
            // Scale Antag_1 to massive size
            antag1.transform.localScale = new Vector3(10f, 10f, 10f);
            Debug.Log("Scaled Antag_1 to massive size");
        }
        
        if (antag2 != null) {
            antag2.SetActive(false);
            Debug.Log("Disabled Antag_2");
        }
        
        if (antag3 != null) {
            antag3.SetActive(false);
            Debug.Log("Disabled Antag_3");
        }
        
        // Remove BattleSetup if it exists
        BattleSetup oldSetup = Object.FindFirstObjectByType<BattleSetup>();
        if (oldSetup != null) {
            DestroyImmediate(oldSetup.gameObject);
        }
        
        // Create BossBattleSetup GameObject
        GameObject bossSetupObj = new GameObject("BossBattleSetup");
        BossBattleSetup bossSetup = bossSetupObj.AddComponent<BossBattleSetup>();
        
        // Auto-assign references
        GameObject protag1 = GameObject.Find("Protag_1");
        GameObject protag2 = GameObject.Find("Protag_2");
        GameObject protag3 = GameObject.Find("Protag_3");
        
        if (protag1 != null) bossSetup.heroObjects[0] = protag1;
        if (protag2 != null) bossSetup.heroObjects[1] = protag2;
        if (protag3 != null) bossSetup.heroObjects[2] = protag3;
        if (antag1 != null) bossSetup.bossObject = antag1;
        
        // Save the scene
        EditorSceneManager.SaveScene(bossScene);
        
        EditorUtility.DisplayDialog("Success", 
            "BossBattleScene created successfully!\n\n" +
            "The scene has been set up with:\n" +
            "- Antag_1 scaled to 10x size\n" +
            "- Antag_2 and Antag_3 disabled\n" +
            "- BossBattleSetup component added",
            "OK");
    }
}
#endif

