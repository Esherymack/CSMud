using System.Collections.Generic;
using System.Xml.Serialization;

/* A 'Room' object is a collection of Things, Entities, and Doors as well as a name 
 * and a description.
 */

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
        public List<XMLReference<Door>> Doors { get; set; }
        [XmlIgnore]
        public List<XMLReference<Entity>> DeadEntities { get; set; }

        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Room()
        {
            Entities = new List<XMLReference<Entity>>();
            Doors = new List<XMLReference<Door>>();
            Things = new List<XMLReference<Thing>>();
            DeadEntities = new List<XMLReference<Entity>>();

            Name = "";
            Id = 0;
            Description = "";
        }
    }
}
