using System.Collections.Generic;

namespace CSMud
{
    public class Map
    {
        public List<Room> Rooms { get; }

        public Map()
        {
            MapBuild mapbuilder = new MapBuild();
            mapbuilder.GenerateMap();
        }
    }
}
