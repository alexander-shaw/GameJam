namespace BattleSystem {
    public class Action {
        public Entity user;
        public ActionType type;
        public Entity target;
        public string description;

        public Action(Entity u = null, ActionType t = ActionType.None, Entity tar = null, string desc = "") {
            user = u;
            type = t;
            target = tar;
            description = desc;
        }
    }
}

