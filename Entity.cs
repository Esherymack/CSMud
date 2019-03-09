using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    public class Entity
    {
        [XmlAttribute]
        private List<string> Commands { get; set; }
        [XmlAttribute]
        private int Id { get; set; }
        [XmlAttribute]
        private string Name { get; set; }
        [XmlAttribute]
        private string Description { get; set; }

        public Entity()
        {
            Commands = null;
            Id = 0;
            Name = "";
            Description = "";
        }
    }
}
