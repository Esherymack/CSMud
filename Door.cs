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
        // The int in [0] is the CURRENT ROOM.
        // The int in [1] is the CONNECTING ROOM.
        // With some randomization, something could be rigged up that a door will send the player to a random room.
        [XmlElement]
        public List<int> RoomsIConnect { get; set; }
        // Determines if a door is 'locked' or 'unlocked'
        [XmlElement]
        public bool Locked { get; set;  }
        // If a door is 'locked,' it must have a key.
        [XmlElement]
        public bool HasKey { get; set; }
        // A door has an ID number
        [XmlElement]
        public int Id { get; set; }
        // A door goes in a direction
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
