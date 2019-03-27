using System;
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

        public Stats(int health, int defense, int percep, int dex, int str)
        {
            Health = health;
            Defense = defense;
            Perception = percep;
            Dexterity = dex;
            Strength = str;
        }
    }
}
