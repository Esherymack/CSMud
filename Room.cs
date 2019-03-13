using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Rooms")]
    public class Room
    { 
        [XmlIgnore]
        public List<Entity> Entities { get; set; }
        [XmlElement]
        public List<XMLReference<Thing>> Thing { get; set; }
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
            Thing = null;

            Name = "";
            Description = "";
        }
    }
}
