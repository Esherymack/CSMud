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
        public List<XMLReference<Enemy>> Enemies { get; set; }
        [XmlElement]
        public List<XMLReference<Door>> Doors { get; set; }

        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Description { get; set; }

        public Room()
        {
            Entities = new List<XMLReference<Entity>>();
            Enemies = new List<XMLReference<Enemy>>();
            Doors = new List<XMLReference<Door>>();
            Things = new List<XMLReference<Thing>>();

            Name = "";
            Id = 0;
            Description = "";
        }
    }
}
