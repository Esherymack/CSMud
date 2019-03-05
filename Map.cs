using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
