using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSMud
{
    /* The stats class houses statistics for players including health pool and damage output. */
    public class Stats
    {
        public int Health { get; set; }
        public int Defense { get; set; }
        public int Perception { get; set; }
        public int Dexterity { get; set; }
        public int Strength { get; set; }
        public int Damage { get; set; }

        public Stats(int health, int defense, int percep, int dex, int str, int damage)
        {
            Health = health;
            Defense = defense;
            Perception = percep;
            Dexterity = dex;
            Strength = str;
            Damage = damage;
        }

        public int getHealth()
        {
            return Health;
        }

        public int getDefense()
        {
            return Defense;
        }

        public int getPerception()
        {
            return Perception;
        }

        public int getDexterity()
        {
            return Dexterity;
        }

        public int getStrength()
        {
            return Strength;
        }
    }
}
