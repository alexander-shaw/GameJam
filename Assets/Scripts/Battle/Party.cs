using System.Collections.Generic;

namespace BattleSystem {
    public class Party {
        public List<Hero> heroes = new List<Hero>();
        public int potions = 5;

        public void AddHero(Hero h) {
            if (heroes.Count < 3)
                heroes.Add(h);
        }

        public bool HasAliveHeroes() {
            foreach (Hero h in heroes) {
                if (h.IsAlive()) return true;
            }
            return false;
        }
    }

    public class EnemyGroup {
        public List<Enemy> enemies = new List<Enemy>();

        public void AddEnemy(Enemy e) {
            if (enemies.Count < 3)
                enemies.Add(e);
        }

        public bool HasAliveEnemies() {
            foreach (Enemy e in enemies) {
                if (e.IsAlive()) return true;
            }
            return false;
        }
    }
}

