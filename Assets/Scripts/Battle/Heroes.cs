namespace BattleSystem {
    // Warrior
    public class Warrior : Hero {
        public Warrior() : base(10, 5, 3, 3) { }
        public override string Ability1Name() { return "Battlecry"; }
        public override string Ability2Name() { return "Brutalize"; }

        public void Battlecry() {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                ModifyAttack(+1);
            }
        }

        public void Brutalize(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                Attack(target);
                target.ModifyDefense(-1);
            }
        }
    }

    // Blandroid
    public class Blandroid : Hero {
        public Blandroid() : base(10, 5, 3, 3) { }
        public override string Ability1Name() { return "Overdrive"; }
        public override string Ability2Name() { return "Brutalize"; }

        public void Overdrive(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                Attack(target);
                ModifyAttack(+1);
            }
        }

        public void Brutalize(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                Attack(target);
                target.ModifyDefense(-1);
            }
        }
    }

    // Mage
    public class Mage : Hero {
        public Mage() : base(6, 3, 1, 5) { }
        public override string Ability1Name() { return "Shield"; }
        public override string Ability2Name() { return "Blast"; }

        public void Shield() {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                ModifyDefense(+2);
            }
        }

        public void Blast(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                target.TakeDamage(GetAttack() * 2);
            }
        }
    }

    // BlinkerBell
    public class BlinkerBell : Hero {
        public BlinkerBell() : base(6, 3, 1, 5) { }
        public override string Ability1Name() { return "Fairy Duster"; }
        public override string Ability2Name() { return "Blaze"; }

        public void Fairyduster(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                Attack(target);
                ModifyDefense(+2);
            }
        }

        public void Blaze(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                target.TakeDamage(GetAttack() * 2);
                target.ModifyAttack(-1);
            }
        }
    }

    // Thief
    public class Thief : Hero {
        public Thief() : base(8, 4, 2, 5) { }
        public override string Ability1Name() { return "Pierce"; }
        public override string Ability2Name() { return "Trick"; }

        public void Pierce(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                target.ModifyHealth(-GetAttack()); // bypasses defense!
            }
        }

        public void Trick(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                target.ModifyDefense(-1);
                ModifyDefense(+1);
            }
        }
    }

    // Jachariah
    public class Jachariah : Hero {
        public Jachariah() : base(8, 4, 2, 5) { }
        public override string Ability1Name() { return "Piercing Shot"; }
        public override string Ability2Name() { return "Multi-Shot"; }

        public void PiercingShot(Entity target) {
            if (GetEnergy() > 0) {
                ModifyEnergy(-1);
                target.ModifyHealth(-GetAttack()); // bypasses defense!
            }
        }

        public void Multishot(Entity target) {
            if (GetEnergy() > 0) {
                Attack(target);
                Attack(target);
            }
        }
    }
}

