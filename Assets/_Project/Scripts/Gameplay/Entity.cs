using UnityEngine;

/// <summary>
/// Base Entity class - represents any game entity with health, energy, defense, and damage
/// Converted from C++ Entity class
/// </summary>
public class Entity : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected int baseHealth;
    [SerializeField] protected int baseEnergy;
    [SerializeField] protected int baseDefense;
    [SerializeField] protected int baseDamage;

    [Header("Current Stats")]
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int currentEnergy;
    [SerializeField] protected int currentDefense;
    [SerializeField] protected int currentDamage;

    public int GetHealth() => currentHealth;
    public int GetEnergy() => currentEnergy;
    public int GetDefense() => currentDefense;
    public int GetAttack() => currentDamage;
    public bool IsAlive() => currentHealth > 0;

    protected virtual void Awake()
    {
        Reset();
    }

    /// <summary>
    /// Reset entity to base stats
    /// </summary>
    public virtual void Reset()
    {
        currentHealth = baseHealth;
        currentEnergy = baseEnergy;
        currentDefense = baseDefense;
        currentDamage = baseDamage;
    }

    /// <summary>
    /// Take damage (reduced by defense)
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        int damage = amount - currentDefense;
        if (damage < 0) damage = 0;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
    }

    /// <summary>
    /// Modify health (healing or damage)
    /// </summary>
    public virtual void ModHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > baseHealth) currentHealth = baseHealth;
        if (currentHealth < 0) currentHealth = 0;
    }

    /// <summary>
    /// Modify energy
    /// </summary>
    public virtual void ModEnergy(int amount)
    {
        currentEnergy += amount;
        if (currentEnergy < 0) currentEnergy = 0;
    }

    /// <summary>
    /// Modify attack damage
    /// </summary>
    public virtual void ModAttack(int amount)
    {
        currentDamage += amount;
        if (currentDamage < 0) currentDamage = 0;
    }

    /// <summary>
    /// Modify defense
    /// </summary>
    public virtual void ModDefense(int amount)
    {
        currentDefense += amount;
        if (currentDefense < 0) currentDefense = 0;
    }

    /// <summary>
    /// Initialize entity with base stats
    /// </summary>
    protected void Initialize(int health, int defense, int damage, int energy)
    {
        baseHealth = health;
        baseDefense = defense;
        baseDamage = damage;
        baseEnergy = energy;
        Reset();
    }
}

