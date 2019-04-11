﻿using System.Collections.Generic;

namespace CSMud
{
    /* The Player object houses the current User's statistics and currently equipped and held Things */
    public class Player
    {
        public List<Thing> Equipped { get; }
        public List<Thing> Held { get; }
        public Stats Stats { get; set; }

        public bool InCombat { get; set; }
        public int ActionPoints { get; set; }

        string Name { get; set; }
        string Appearance { get; set; }

        public Player(string name)
        {
            Equipped = new List<Thing>();
            Held = new List<Thing>();
            Stats = new Stats(100, 25, 15, 15, 15, 0, 15, 15, 15, 5, 15);
            InCombat = false;
            ActionPoints = 1;
            Name = name;
        }

        // Equip a wearable item
        public void Equip(Thing thing)
        {
            Equipped.Add(thing);
        }

        // Unequip a worn item
        public void Unequip(Thing thing)
        {
            Equipped.Remove(thing);
        }

        // Hold an item in your hand
        public void Hold(Thing thing)
        {
            Held.Add(thing);
        }

        // Drop a held item.
        public void Drop(Thing thing)
        {
            Held.Remove(thing);
        }
    }
}
