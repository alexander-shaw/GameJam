using UnityEngine;

/// <summary>
/// Mage hero class - low health, medium damage, low defense, high energy
/// Converted from C++ Mage class
/// </summary>
public class Mage : Hero
{
    void Awake()
    {
        InitializeHero("Mage", 6, 3, 1, 5);
    }

    /// <summary>
    /// Shield: Increase defense by 2, costs 1 energy
    /// </summary>
    public void Shield()
    {
        if (currentEnergy > 0)
        {
            ModEnergy(-1);
            ModDefense(2);
        }
    }

    /// <summary>
    /// Blast: Deal double damage to target, costs 1 energy
    /// </summary>
    public void Blast(Entity target)
    {
        if (target != null && currentEnergy > 0)
        {
            ModEnergy(-1);
            target.TakeDamage(currentDamage * 2);
        }
    }
}

