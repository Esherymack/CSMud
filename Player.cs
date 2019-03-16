using System;
using System.Collections.Generic;

namespace CSMud
{
    /* The Player object houses the current User's statistics and currently equipped and held Things
     */   
    public class Player
    {
        public List<Thing> Equipped { get; }
        public List<Thing> Held { get; }
        public Stats Stats { get; set; }

        string Name { get; set; }
        string Appearance { get; set; }

        public Player(string name)
        {
            Equipped = new List<Thing>();
            Held = new List<Thing>();
            Stats = new Stats();
            Name = name;
        }

        public void Equip(Thing thing)
        {
            Equipped.Add(thing);
        }
        public void Unequip(Thing thing)
        {
            Equipped.Remove(thing);
        }

        public void Hold(Thing thing)
        {
            Held.Add(thing);
        }

        public bool Drop(Thing thing)
        {
            return Held.Remove(thing);
        }
    }
}
