using UnityEngine;

namespace BattleSystem {
    public class Entity {
        protected int currentHealth;
        protected int currentEnergy;
        protected int currentDefense;
        protected int currentDamage;

        protected int normalHealth;
        protected int normalEnergy;
        protected int normalDefense;
        protected int normalDamage;

        public Entity(int hp, int dmg, int def, int energy) {
            normalHealth = hp;
            normalDamage = dmg;
            normalDefense = def;
            normalEnergy = energy;
            Reset();
        }

        public virtual void Reset() {
            currentHealth = normalHealth;
            currentDamage = normalDamage;
            currentDefense = normalDefense;
            currentEnergy = normalEnergy;
        }

        // --- Combat Functions ---

        public void TakeDamage(int amount) {
            int dmg = amount - currentDefense;
            if (dmg < 1) dmg = 1; // Always do at least 1 damage
            currentHealth -= dmg;
            if (currentHealth < 0) currentHealth = 0;
        }

        public void ModifyHealth(int amount) {
            currentHealth += amount;
            if (currentHealth > normalHealth) currentHealth = normalHealth;
            if (currentHealth < 0) currentHealth = 0;
        }

        public void ModifyEnergy(int amount) {
            currentEnergy += amount;
            if (currentEnergy < 0) currentEnergy = 0;
        }

        public void ModifyAttack(int amount) {
            currentDamage += amount;
            if (currentDamage < 0) currentDamage = 0;
        }

        public void ModifyDefense(int amount) {
            currentDefense += amount;
            if (currentDefense < 0) currentDefense = 0;
        }

        public void Attack(Entity target) {
            target.TakeDamage(currentDamage);
        }

        public bool IsAlive() { return currentHealth > 0; }

        public int GetEnergy() { return currentEnergy; }
        public int GetAttack() { return currentDamage; }
        public int GetHealth() { return currentHealth; }
        public int GetDefense() { return currentDefense; }
        public int GetMaxHealth() { return normalHealth; }
        public int GetMaxEnergy() { return normalEnergy; }
    }
}

