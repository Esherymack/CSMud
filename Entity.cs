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

        // TODO: figure out way to add an inventory ("drop inventory" for enemies, "shop inventory" for npcs)

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
        // Whether or not the entity is friendly
        [XmlElement]
        public bool IsFriendly { get; set; }
        // Whether or not the entity is hidden
        [XmlElement]
        public bool IsHidden { get; set; }
        [XmlElement]
        public int minPerception { get; set; }

        public Entity()
        {
            Commands = new List<string>();
            Id = 0;
            Name = "";
            Description = "";
            Health = 100;
            Defense = 100;
            Damage = 10;
            IsFriendly = true;
            IsHidden = false;
            minPerception = 0;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
