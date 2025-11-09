using UnityEngine;

/// <summary>
/// Base Hero class - extends Entity with hero-specific functionality
/// Converted from C++ Hero class
/// </summary>
public class Hero : Entity
{
    [Header("Hero Info")]
    public string heroName;

    /// <summary>
    /// Initialize hero with stats
    /// </summary>
    protected void InitializeHero(string name, int health, int damage, int defense, int energy)
    {
        heroName = name;
        Initialize(health, defense, damage, energy);
    }

    /// <summary>
    /// Basic attack on an enemy
    /// </summary>
    public virtual void Attack(Entity target)
    {
        if (target != null && currentEnergy > 0)
        {
            ModEnergy(-1);
            target.TakeDamage(currentDamage);
        }
    }
}

