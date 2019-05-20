namespace CSMud
{
    /* The stats class houses statistics for players including health pool and damage output. */
    public class Stats
    {
        // The player's health determines how many hit points (HP) they have.
        public int MaxHealth { get; set; }
        public int CurrHealth { get; set; }
        // The player's defense determines damage reduction from incoming attacks.
        public int Defense { get; set; }
        // The player's perception determines their ability to see hidden enemies and traps.
        public int Perception { get; set; }
        // The player's dexterity determines their ability to pick locks and dismantle traps.
        public int Dexterity { get; set; }
        // The player's strength determines their damage multiplier on critical hits and their inventory capacity.
        public int Strength { get; set; }
        // The player's damage determines how much base damage an attack does on any given enemy.
        public int Damage { get; set; }
        // The player's accuracy helps modify whether or not attacks hit enemies
        public int Accuracy { get; set; }
        // The player's agility helps modify whether or not enemies hit the player.
        public int Agility { get; set; }
        // The player's luck helps modify critical hit chance, dodge chance, and chance of an enemy not picking them in combat.
        public int Luck { get; set; }
        // The player's presence is a hidden stat that helps determine turn order in combat, as well as help enemies pick who to attack.
        public int Presence { get; set; }
        // Player's crit avoid is a hidden stat that helps determine their likelihood of avoiding a critical hit
        public int CritAvoid { get; set; }
        // The player's knowledge helps determine spell damage, as well as make better conversation.
        public int Knowledge { get; set; }

        public Stats(int health, int defense, int percep, int dex, int str, int damage, int acc, int agil, int luck, int know)
        {
            MaxHealth = health;
            CurrHealth = MaxHealth;
            Defense = defense;
            Perception = percep;
            Dexterity = dex;
            Strength = str;
            Damage = damage;
            Accuracy = acc;
            Agility = agil;
            Luck = luck;
            Knowledge = know;

            Presence = Agility + Dexterity + Accuracy + Knowledge - Defense - Luck - Strength;
            CritAvoid = (Agility + Dexterity + Accuracy + Perception + Luck) / 2;
        }
    }
}
