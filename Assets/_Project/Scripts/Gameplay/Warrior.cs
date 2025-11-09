using UnityEngine;

/// <summary>
/// Warrior hero class - high health, medium damage, good defense
/// Converted from C++ Warrior class
/// </summary>
public class Warrior : Hero
{
    void Awake()
    {
        InitializeHero("Warrior", 10, 5, 3, 3);
    }

    /// <summary>
    /// Battlecry: Increase attack by 1, costs 1 energy
    /// </summary>
    public void Battlecry()
    {
        if (currentEnergy > 0)
        {
            ModEnergy(-1);
            ModAttack(1);
        }
    }

    /// <summary>
    /// Brutalize: Attack enemy and reduce their defense by 1, costs 1 energy
    /// </summary>
    public void Brutalize(Entity enemy)
    {
        if (enemy != null && currentEnergy > 0)
        {
            ModEnergy(-1);
            Attack(enemy);
            enemy.ModDefense(-1);
        }
    }
}

