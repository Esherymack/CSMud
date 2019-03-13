using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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

        public Entity()
        {
            Commands = null;
            Id = 0;
            Name = "";
            Description = "";
        }

        public override string ToString()
        {
            return $"{Name}\t{Description}\t{Commands.Aggregate((a, b) => $"{a}, {b}")}\n";
        }
    }
}
