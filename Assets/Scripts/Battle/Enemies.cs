using UnityEngine;

namespace BattleSystem {
    // Helper functions for enemy AI
    public static class EnemyAIHelpers {
        public static Hero GetRandomHero(Party party) {
            System.Collections.Generic.List<Hero> alive = new System.Collections.Generic.List<Hero>();
            foreach (Hero h in party.heroes) {
                if (h.IsAlive()) alive.Add(h);
            }

            if (alive.Count == 0) return null;
            return alive[Random.Range(0, alive.Count)];
        }

        public static Hero GetHeroLowestHP(Party party) {
            Hero best = null;
            foreach (Hero h in party.heroes) {
                if (h.IsAlive() && (best == null || h.GetHealth() < best.GetHealth()))
                    best = h;
            }
            return best;
        }

        public static Hero GetHeroHighestAttack(Party party) {
            Hero best = null;
            foreach (Hero h in party.heroes) {
                if (h.IsAlive() && (best == null || h.GetAttack() > best.GetAttack()))
                    best = h;
            }
            return best;
        }
    }

    // Goblin
    public class Goblin : Enemy {
        public Goblin() : base("Goblin", 6, 3, 1, 3) { }
        public override string AbilityName() { return "Trick"; }

        public override Action DecideAction(Party party) {
            // Target will be set by BattleManager to a random hero
            // Just decide the action type
            Hero target = EnemyAIHelpers.GetRandomHero(party);
            if (target == null) return new Action(this, ActionType.None, null);

            bool useAbility = (Random.Range(0, 2) == 0);
            if (useAbility)
                return new Action(this, ActionType.Ability1, target, AbilityName());
            return new Action(this, ActionType.Attack, target, "attacks");
        }

        public void Ability(Entity target) {
            target.ModifyDefense(-1);
            ModifyDefense(+1);
        }
    }

    // Orc
    public class Orc : Enemy {
        public Orc() : base("Orc", 12, 5, 3, 3) { }
        public override string AbilityName() { return "Brutalize"; }

        public override Action DecideAction(Party party) {
            Hero target = EnemyAIHelpers.GetRandomHero(party);
            if (target == null) return new Action(this, ActionType.None, null);

            if (GetHealth() < (GetMaxHealth() / 2))
                return new Action(this, ActionType.Ability1, target, AbilityName());

            return new Action(this, ActionType.Attack, target, "attacks");
        }

        public void Ability(Entity target) {
            Attack(target);
            target.ModifyDefense(-1);
        }
    }

    // Ghoul
    public class Ghoul : Enemy {
        public Ghoul() : base("Ghoul", 14, 6, 1, 3) { }
        public override string AbilityName() { return "Drain Bite"; }

        public override Action DecideAction(Party party) {
            // Always target random hero (BattleManager will override)
            Hero target = EnemyAIHelpers.GetRandomHero(party);
            if (target == null) return new Action(this, ActionType.None, null);

            if (GetHealth() < (GetMaxHealth() / 2))
                return new Action(this, ActionType.Ability1, target, AbilityName());

            return new Action(this, ActionType.Attack, target, "attacks");
        }

        public void Ability(Entity target) {
            target.ModifyHealth(-GetAttack());
            ModifyHealth(+GetAttack());
        }
    }

    // Warlock
    public class Warlock : Enemy {
        private bool usedFirstTurn = false;

        public Warlock() : base("Warlock", 8, 7, 1, 4) { }
        public override string AbilityName() { return "Burst"; }

        public override Action DecideAction(Party party) {
            // Always target random hero (BattleManager will override)
            Hero target = EnemyAIHelpers.GetRandomHero(party);
            if (target == null) return new Action(this, ActionType.None, null);

            if (!usedFirstTurn) {
                usedFirstTurn = true;
                return new Action(this, ActionType.Ability1, target, AbilityName());
            }

            return new Action(this, ActionType.Attack, target, "attacks");
        }

        public void Ability(Entity target) {
            target.TakeDamage(GetAttack() * 2);
        }
    }

    // Dragon
    public class Dragon : Enemy {
        private int phaseStep = 0; // 0 = Flame Roar, 1 = Burst, 2 = Claw → repeat 1,2
        private int maxAtk, maxDef;

        public Dragon() : base("Dragon", 40, 8, 4, 0) {
            maxAtk = GetAttack();
            maxDef = GetDefense();
        }

        public override string AbilityName() {
            return "Dragon Ability";
        }

        public override Action DecideAction(Party party) {
            // Emergency Stat Recovery Check
            if (GetAttack() < maxAtk / 2 || GetDefense() < maxDef / 2) {
                return new Action(this, ActionType.Ability1, null, "Ancient Resurgence");
            }

            // Phase Rotation - but BattleManager will override target to be random hero
            Hero randomHero = EnemyAIHelpers.GetRandomHero(party);
            
            if (phaseStep == 0) {
                phaseStep = 1;
                return new Action(this, ActionType.Ability1, randomHero, "Flame Roar");
            }
            else if (phaseStep == 1) {
                phaseStep = 2;
                // Target will be overridden by BattleManager, but we'll use random hero for now
                return new Action(this, ActionType.Ability1, randomHero, "Draconic Burst");
            }
            else {
                phaseStep = 1; // loop back
                return new Action(this, ActionType.Ability1, randomHero, "Heavy Claw");
            }
        }

        public void Ability(Entity rawTarget, Party party) {
            // Emergency Resurgence
            if (GetAttack() < maxAtk / 2 || GetDefense() < maxDef / 2) {
                Debug.Log("The Dragon restores its strength!");
                ModifyAttack(maxAtk - GetAttack());
                ModifyDefense(maxDef - GetDefense());
                return;
            }

            // Flame Roar (AOE, no target)
            if (rawTarget == null && phaseStep == 1) {
                Debug.Log("The Dragon unleashes FLAME ROAR!");
                foreach (Hero h in party.heroes) {
                    if (!h.IsAlive()) continue;
                    h.TakeDamage(GetAttack() - 2);
                    h.ModifyDefense(-1);
                }
                return;
            }

            // Draconic Burst (Target highest HP)
            if (phaseStep == 2) {
                Debug.Log($"The Dragon casts DRACONIC BURST on {rawTarget.GetType().Name}!");
                rawTarget.TakeDamage(GetAttack() * 2);
                return;
            }

            // Heavy Claw (Random single target)
            Debug.Log("The Dragon uses HEAVY CLAW!");
            rawTarget.TakeDamage(GetAttack());
        }
    }
}

