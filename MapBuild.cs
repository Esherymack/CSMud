using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace CSMud
{
    public class MapBuild
    {
        public void GenerateMap()
        {
            Console.WriteLine("Generating map...");
            Console.WriteLine("\n");
            List<Thing> generatedThings = CreateThing();
            List<Entity> generatedEntities = CreateEntity();
            CreateRoom(generatedThings, generatedEntities);
            Console.WriteLine("\n");
            Console.WriteLine("Map generated!");
        }

        public List<Thing> CreateThing()
        {
            List<Thing> Things = new List<Thing>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Thing>), new XmlRootAttribute("Things"));
            XmlReader reader = XmlReader.Create(@"..\..\data\thing.xml");
            Things = (List<Thing>)serializer.Deserialize(reader);
            reader.Close();
            return Things;
        }

        public List<Entity> CreateEntity()
        {
            List<Entity> Entities = new List<Entity>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entity>), new XmlRootAttribute("Entities"));
            XmlReader reader = XmlReader.Create(@"..\..\data\entity.xml");
            Entities = (List<Entity>)serializer.Deserialize(reader);
            reader.Close();
            return Entities;
        }

        public void CreateRoom(List<Thing> things, List<Entity> entities)
        {
            List<Room> Rooms = new List<Room>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Room>), new XmlRootAttribute("Rooms"));
            XmlReader reader = XmlReader.Create(@"..\..\data\room.xml");
            Rooms = (List<Room>)serializer.Deserialize(reader);
            Rooms.ForEach(i => XMLReference<Thing>.Link(i.Thing, things));
            Rooms.ForEach(i => Console.Write($"{i.Doors.Aggregate((a, b) => $"{a}, {b}")}\t{i.Name}\t{i.Description}\n{i.Thing}\n"));
            reader.Close();        
        }
    }
}
