using UnityEngine;

/// <summary>
/// Thief hero class - medium health, medium damage, medium defense, high energy
/// Converted from C++ Thief class
/// </summary>
public class Thief : Hero
{
    void Awake()
    {
        InitializeHero("Thief", 8, 4, 2, 5);
    }

    /// <summary>
    /// Pierce: Deal damage directly to health (ignores defense), costs 1 energy
    /// </summary>
    public void Pierce(Entity target)
    {
        if (target != null && currentEnergy > 0)
        {
            ModEnergy(-1);
            target.ModHealth(-currentDamage);
        }
    }

    /// <summary>
    /// Trick: Reduce target's defense by 1 and increase own defense by 1, costs 1 energy
    /// </summary>
    public void Trick(Entity target)
    {
        if (target != null && currentEnergy > 0)
        {
            ModEnergy(-1);
            target.ModDefense(-1);
            ModDefense(1);
        }
    }
}

