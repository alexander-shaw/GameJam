using UnityEngine;

/// <summary>
/// Enemy entity class - extends Entity with enemy-specific functionality
/// Converted from C++ Enemy class
/// </summary>
public class EnemyEntity : Entity
{
    [Header("Enemy Info")]
    public string enemyName;

    /// <summary>
    /// Initialize enemy with stats
    /// </summary>
    public void InitializeEnemy(string name, int health, int damage, int defense, int energy)
    {
        enemyName = name;
        Initialize(health, defense, damage, energy);
    }

    /// <summary>
    /// Create a basic enemy with default stats
    /// </summary>
    public static EnemyEntity CreateBasicEnemy(string name)
    {
        GameObject enemyObj = new GameObject(name);
        EnemyEntity enemy = enemyObj.AddComponent<EnemyEntity>();
        enemy.InitializeEnemy(name, 5, 3, 1, 2);
        return enemy;
    }
}

