using System;
using System.Collections.Generic;
namespace CSMud
{
    public class Room
    {
        public List<Thing> Things { get; }
        public List<Entity> Entities { get; }
        public List<char> Doors { get; set; }

        private string Id { get; set; }
        private string Name { get; set; }
        private string Description { get; set; }

        public Room(string name, string description, string id, List<char> directions, List<Thing> things, List<Entity> entities)
        {
            this.Doors = directions;
            this.Things = things;
            this.Entities = entities;

            this.Name = name;
            this.Id = id;
            this.Description = description;
        }
    }
}
