using System.Collections.Generic;

namespace CSMud
{
    /* The map is a collection of Room objects
     * Room objects are defined by the Room class - see docs there for specific
     *      documentation on these.
     * All players start in the first room in the list.
     */
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
