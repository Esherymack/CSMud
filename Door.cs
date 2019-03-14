using System.Collections.Generic;
using System.Xml.Serialization;

/* A 'Door' object is a collection of room ID numbers connected by another ID to allow 
 * nonlinear map construction. Also allows flagging if doors are locked and have keys. */

namespace CSMud
{
    [XmlRoot("Doors")]
    public class Door : Identifiable
    {
        // A RoomsIConnect list is typically only of length 2
        // The int in [0] is the CURRENT ROOM
        // the int in [1] is the CONNECTING ROOM
        [XmlElement]
        public List<int> RoomsIConnect { get; set; }
        [XmlElement]
        public bool Locked { get; set;  }
        [XmlElement]
        public bool HasKey { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Direction { get; set; }

        public Door()
        {
            Direction = "";
            Locked = false;
            HasKey = false;
            Id = 0;
        }

        public override string ToString()
        {
            return $"{Direction}";
        }
    }
}
