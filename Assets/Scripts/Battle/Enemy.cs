namespace BattleSystem {
    public abstract class Enemy : Entity {
        protected string enemyName;

        public Enemy(string n, int hp, int atk, int def, int energy)
            : base(hp, atk, def, energy) {
            enemyName = n;
        }

        public string GetName() { return enemyName; }

        public abstract string AbilityName();
        public abstract Action DecideAction(Party party);
    }
}

