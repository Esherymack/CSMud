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
        public int Level { get; set; }
        public int Experience { get; set; }

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
            Level = 1;
            Experience = 0;
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

        public void LevelUp()
        {
            Level++;
        }

        // Equip a wearable item
        public void Equip(Item item)
        {
            Equipped.Add(item);
        }

        // Unequip a worn item
        public void Unequip(Item item)
        {
            Equipped.Remove(item);
        }

        // Hold an item in your hand
        public void Hold(Item item)
        {
            Held.Add(item);
        }

        // Drop a held item.
        public void Drop(Item item)
        {
            Held.Remove(item);
        }
    }
}
