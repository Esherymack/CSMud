using System.Collections.Generic;

namespace CSMud
{
    public class Map
    {
        public List<Room> Rooms { get; }

        public Map(List<Room> rooms)
        {
            this.Rooms = rooms;
        }
    }
}
