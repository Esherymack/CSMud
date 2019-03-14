using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

/* A 'Thing' object is a collection of commands, an Id, a Name, and a Description
 */

namespace CSMud
{
    [XmlRoot("Things")]
    public class Thing : Identifiable
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

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
