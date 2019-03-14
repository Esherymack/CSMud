using System.Collections.Generic;
using System.Linq;
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
        [XmlElement]
        public List<string> Commands { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public bool IsFriendly { get; set; }

        public Entity()
        {
            Commands = null;
            Id = 0;
            Name = "";
            Description = "";
            IsFriendly = true;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
