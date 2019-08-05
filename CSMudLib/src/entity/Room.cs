using System.Collections.Generic;
using System.Xml.Serialization;
using CSMud.Utils;

/* A 'Room' object is a collection of Things, Entities, and Doors as well as a name 
 * and a description.
 */

namespace CSMud.Entity
{
    [XmlRoot("Rooms")]
    public class Room 
    { 
        [XmlElement]
        public List<XMLReference<Item>> Items { get; set; }
        [XmlElement]
        public List<XMLReference<NPC>> NPCs { get; set; }
        [XmlElement]
        public List<XMLReference<Door>> Doors { get; set; }
        [XmlIgnore]
        public List<XMLReference<NPC>> DeadNPCs { get; set; }


        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public string Ambient { get; set; }

        public Room()
        {
            NPCs = new List<XMLReference<NPC>>();
            Doors = new List<XMLReference<Door>>();
            Items = new List<XMLReference<Item>>();
            DeadNPCs = new List<XMLReference<NPC>>();

            Name = "";
            Id = 0;
            Description = "";
            Ambient = "";
        }
    }
}
