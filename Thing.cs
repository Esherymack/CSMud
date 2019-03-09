using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    public class Thing
    {
        [XmlAttribute]
        public List<string> Commands { get; set; }
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
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
