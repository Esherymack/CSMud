using System.Collections.Generic;
using System.Xml.Serialization;

namespace CSMud
{
    public class Room
    {
        public List<Thing> Things { get; }
        public List<Entity> Entities { get; }
        [XmlAttribute]
        public List<char> Doors { get; set; }

        [XmlAttribute]
        private int Id { get; set; }
        [XmlAttribute]
        private string Name { get; set; }
        [XmlAttribute]
        private string Description { get; set; }

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
