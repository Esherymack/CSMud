using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Things")]
    public class Thing
    {
        [XmlElement]
        public List<string> Commands { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Thing()
        {
            Commands = null;
            Id = 0;
            Name = "";
            Description = "";
        }
    }
}
