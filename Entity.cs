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
        // Whether or not the entity is friendly
        [XmlElement]
        public bool IsFriendly { get; set; }
        // Whether or not the entity is hidden
        [XmlElement]
        public bool IsHidden { get; set; }

        public Entity()
        {
            Commands = new List<string>();
            Id = 0;
            Name = "";
            Description = "";
            IsFriendly = true;
            IsHidden = false;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
