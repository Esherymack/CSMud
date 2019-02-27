using System;
using System.Collections.Generic;
namespace CSMud
{
    public class Room
    {
        public List<Objects> Objects { get; }
        public List<Entity> Entities { get; }
        public List<Doors> Doors { get; }
        public Room()
        {
        }
    }
}
