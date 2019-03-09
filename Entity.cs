using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Entities")]
    public class Entity
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
    }
}
