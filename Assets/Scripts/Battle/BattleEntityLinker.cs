using UnityEngine;
using BattleSystem;

/// <summary>
/// Links a GameObject to an Entity in the battle system
/// Attach this to Protag and Antag GameObjects to link them to their Entity counterparts
/// </summary>
public class BattleEntityLinker : MonoBehaviour {
    [Header("Entity Reference")]
    [Tooltip("The Entity this GameObject represents (Hero or Enemy)")]
    public Entity linkedEntity;
    
    [Header("Entity Type")]
    [Tooltip("Is this a Hero (true) or Enemy (false)?")]
    public bool isHero = false;
    
    [Header("Index")]
    [Tooltip("Index in party (0-2 for heroes) or enemy group (0-2 for enemies)")]
    public int entityIndex = -1;
    
    private DamageFlashEffect flashEffect;
    
    void Awake() {
        flashEffect = GetComponent<DamageFlashEffect>();
        if (flashEffect == null) {
            flashEffect = gameObject.AddComponent<DamageFlashEffect>();
        }
    }
    
    /// <summary>
    /// Flash this entity when it takes damage
    /// </summary>
    public void FlashOnDamage() {
        if (flashEffect != null) {
            flashEffect.Flash();
        }
    }
    
    /// <summary>
    /// Get the linked Entity
    /// </summary>
    public Entity GetEntity() {
        return linkedEntity;
    }
}

