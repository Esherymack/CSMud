using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Rooms")]
    public class Room
    {
        [XmlIgnore]
        public List<Thing> Things { get; set; }
        [XmlIgnore]
        public List<Entity> Entities { get; set; }
        [XmlElement]
        public List<string> Doors { get; set; }

        [XmlElement]
        public XMLReference<Thing> Thing { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Room()
        {
            Things = null;
            Entities = null;
            Doors = null;

            Thing = null;
            Name = "";
            Description = "";
        }
    }
}
