using System.Collections.Generic;

namespace CSMud
{
    /* The Player object houses the current User's statistics and currently equipped and held Things */
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
            Stats = new Stats(100, 25, 15, 15, 15);
            Name = name;
        }

        // Equip a wearable item
        // TODO: figure out what to do here and in Hold when the list of items is full
        public void Equip(Thing thing)
        {
            if (Equipped.Count == 6)
            {}
            // TODO: check type of thing to wear - if already worn, do not equip immediately, but ask to flip
            else
            {
                Equipped.Add(thing);
            }
        }

        // Unequip a worn item
        public void Unequip(Thing thing)
        {
            Equipped.Remove(thing);
        }

        // Hold an item in your hand
        // TODO: see TODO on line 23
        public void Hold(Thing thing)
        {
            if (Held.Count == 2)
            {}
            else
            {
                Held.Add(thing);
            }
        }

        // Drop a held item.
        public void Drop(Thing thing)
        {
            Held.Remove(thing);
        }
    }
}
