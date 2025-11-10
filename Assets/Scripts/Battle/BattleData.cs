using UnityEngine;

/// <summary>
/// Static class to pass battle data between scenes
/// </summary>
public static class BattleData {
    public static string primaryEnemySpriteName = "";
    public static Sprite primaryEnemySprite = null;
    public static bool isBossBattle = false;
    public static string enemyToRemoveName = ""; // Track which enemy GameObject to remove
    
    public static void SetPrimaryEnemy(Sprite sprite) {
        primaryEnemySprite = sprite;
        if (sprite != null) {
            primaryEnemySpriteName = sprite.name;
            // Check if it's a boss battle
            isBossBattle = sprite.name.ToLower().Contains("cyberdragon") || 
                          sprite.name.ToLower().Contains("cyber-dragon");
        }
    }
    
    public static void SetEnemyToRemove(string enemyName) {
        enemyToRemoveName = enemyName;
    }
    
    public static void Clear() {
        primaryEnemySprite = null;
        primaryEnemySpriteName = "";
        isBossBattle = false;
        enemyToRemoveName = "";
    }
}

