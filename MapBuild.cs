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
            CreateThing();
            Console.WriteLine("\n");
            CreateEntity();
            Console.WriteLine("\n");
            CreateRoom();
            Console.WriteLine("\n");
            Console.WriteLine("Map generated!");
        }

        public void CreateThing()
        {
            List<Thing> Things = new List<Thing>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Thing>), new XmlRootAttribute("Things"));
            XmlReader reader = XmlReader.Create(@"..\..\data\thing.xml");
            Things = (List<Thing>)serializer.Deserialize(reader);
            Things.ForEach(i => Console.Write($"{i.Name}\t{i.Id}\t{i.Description}\t{i.Commands.Aggregate((a, b) => $"{a}, {b}")}\n"));
            reader.Close();
        }

        public void CreateEntity()
        {
            List<Entity> Entities = new List<Entity>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entity>), new XmlRootAttribute("Entities"));
            XmlReader reader = XmlReader.Create(@"..\..\data\entity.xml");
            Entities = (List<Entity>)serializer.Deserialize(reader);
            Entities.ForEach(i => Console.Write($"{i.Name}\t{i.Id}\t{i.Description}\t{i.Commands.Aggregate((a, b) => $"{a}, {b}")}\n"));
            reader.Close();
        }

        public void CreateRoom()
        {
            List<Room> Rooms = new List<Room>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Room>), new XmlRootAttribute("Rooms"));
            XmlReader reader = XmlReader.Create(@"..\..\data\room.xml");
            Rooms = (List<Room>)serializer.Deserialize(reader);
            Rooms.ForEach(i => Console.Write($"{i.Doors.Aggregate((a, b) => $"{a}, {b}")}\t{i.Id}\t{i.Name}\t{i.Description}\n"));
            reader.Close();
        }
    }
}
