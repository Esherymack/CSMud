using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSMud
{
    /* The stats class houses statistics for players including health pool and damage output. */
    public class Stats
    {
        // The player's health determines how many hit points (HP) they have.
        public int Health { get; set; }
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
        // The highest presence can be is 15.
        // Base presence is 5.
        public int Presence { get; set; }
        // The player's knowledge helps determine spell damage, as well as make better conversation.
        public int Knowledge { get; set; }

        public Stats(int health, int defense, int percep, int dex, int str, int damage, int acc, int agil, int luck, int pres, int know)
        {
            Health = health;
            Defense = defense;
            Perception = percep;
            Dexterity = dex;
            Strength = str;
            Damage = damage;
            Accuracy = acc;
            Agility = agil;
            Luck = luck;
            Presence = pres;
            Knowledge = know;
        }
    }
}
