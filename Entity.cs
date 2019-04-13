using System.Collections.Generic;
using System.Xml.Serialization;

/* Entity class for CSMud
 * An entity is any NPC
 * Entities can be friendly or enemies
 * Entities have specific sets of commands, unique ID Numbers, a name, and a description.
 */

namespace CSMud
{
    [XmlRoot("Entities")]
    public class Entity : Identifiable
    {
        // Valid commands for a given entity
        [XmlElement]
        public List<string> Commands { get; set; }
        // ID number of an entity
        [XmlElement]
        public int Id { get; set; }
        // The entity's name
        [XmlElement]
        public string Name { get; set; }
        // The entity's 'examine' description
        [XmlElement]
        public string Description { get; set; }
        // Entity's health level
        [XmlElement]
        public int Health { get; set; }
        // Entity's defense level
        [XmlElement]
        public int Defense { get; set; }
        // Entities deal implicit damage regardless of disposition to the player
        [XmlElement]
        public int Damage { get; set; }
        // Entity faction
        [XmlElement]
        public string Faction { get; set; }
        // Whether or not the entity is hidden
        [XmlElement]
        public bool IsHidden { get; set; }
        // If an entity is hidden, their minimum perception rating determines if the player can see them.
        [XmlElement]
        public int minPerception { get; set; }
        [XmlElement]
        public int MinStrike { get; set; }
        // An entity's Presence score is to help place them in the order of turns.
        [XmlElement]
        public int Presence { get; set; }
        // The entity's inventory
        [XmlElement]
        List<XMLReference<Thing>> Inventory { get; set; }

        [XmlIgnore]
        public Combat Combat { get; set; }
        [XmlIgnore]
        public bool InCombat { get; set; }
        [XmlIgnore]
        public bool IsDead { get; set; }


        public Entity()
        {
            Commands = new List<string>();
            Id = 0;
            Name = "";
            Description = "";
            Health = 100;
            Defense = 100;
            Damage = 10;
            Faction = "ally";
            IsHidden = false;
            minPerception = 0;
            MinStrike = 0;
            Presence = 0;
            Inventory = new List<XMLReference<Thing>>();
            InCombat = false;
            IsDead = false;
        }

        public void setHealthLowered(int damage)
        {
            Health = Health - damage;
        }

        public void setHealthRaised(int heal)
        {
            Health = Health + heal;
        }

        public void setDefenseLowered(int damage)
        {
            Defense = Defense - damage;
        }

        public void setDefenseRaised(int restore)
        {
            Defense = Defense + restore;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
