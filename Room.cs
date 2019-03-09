using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    [XmlRoot("Rooms")]
    public class Room
    {
        [XmlIgnore]
        public List<Thing> Things { get; }
        [XmlIgnore]
        public List<Entity> Entities { get; }
        [XmlElement]
        public List<string> Doors { get; set; }

        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Room()
        {
            Things = null;
            Entities = null;
            Doors = null;

            Id = 0;
            Name = "";
            Description = "";
        }
    }
}
