using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Rooms")]
    public class Room
    { 
        [XmlElement]
        public List<XMLReference<Thing>> Things { get; set; }
        [XmlElement]
        public List<XMLReference<Entity>> Entities { get; set; }
        [XmlElement]
        public List<string> Doors { get; set; }

        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Room()
        {
            Entities = null;
            Doors = null;
            Things = null;

            Name = "";
            Description = "";
        }
    }
}
