using System.Collections.Generic;
using CSMud.Events;
using CSMud.Entity;

namespace CSMud.Client
{
    /* The Player object houses the current User's statistics and currently equipped and held Items */
    public class Player
    {
        public List<Item> Equipped { get; }
        public List<Item> Held { get; }
        public Stats Stats { get; set; }

        public Combat Combat { get; set; }
        public Conversation Conversation { get; set; }
        public bool IsBlocking { get; set; }
        public bool IsDead { get; set; }
        public bool IsTrading { get; set; }

        string Name { get; set; }
        string Appearance { get; set; }


        public Player(string name)
        {
            Equipped = new List<Item>();
            Held = new List<Item>();
            IsBlocking = false;
            IsDead = false;
            IsTrading = false;
            // Stats have a maximum of 100, except for health and defense.
            Stats = new Stats(100, 5, 5, 5, 5, 5, 5, 5, 5, 5);
            Name = name;
        }

        public void TakeDamage(int damage)
        {
            Stats.CurrHealth = Stats.CurrHealth - damage;
        }

        public void Heal(int heal)
        {
            Stats.CurrHealth = Stats.CurrHealth + heal;
            if(Stats.CurrHealth > Stats.MaxHealth)
            {
                Stats.CurrHealth = Stats.MaxHealth;
            }
        }

        // Equip a wearable item
        public void Equip(Item thing)
        {
            Equipped.Add(thing);
        }

        // Unequip a worn item
        public void Unequip(Item thing)
        {
            Equipped.Remove(thing);
        }

        // Hold an item in your hand
        public void Hold(Item thing)
        {
            Held.Add(thing);
        }

        // Drop a held item.
        public void Drop(Item thing)
        {
            Held.Remove(thing);
        }
    }
}
