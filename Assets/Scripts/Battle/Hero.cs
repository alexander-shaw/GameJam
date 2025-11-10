namespace BattleSystem {
    public abstract class Hero : Entity {
        public Hero(int hp, int atk, int def, int energy) : base(hp, atk, def, energy) { }

        public abstract string Ability1Name();
        public abstract string Ability2Name();
    }
}

