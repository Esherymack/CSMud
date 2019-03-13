﻿using System;
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
            using (XmlReader reader = XmlReader.Create(@"..\..\data\room.xml"))
            {
                Rooms = (List<Room>)serializer.Deserialize(reader);
                Rooms.ForEach(i => XMLReference<Thing>.Link(i.Things, things));
                Rooms.ForEach(i => XMLReference<Entity>.Link(i.Entities, entities));
                Rooms.ForEach(r => printRoomDescription(r));
            }
        }

        void printRoomDescription(Room room)
        {
            Console.WriteLine($"{room.Doors.Aggregate((a, b) => $"{a}, {b}")}\t{room.Name}\t{room.Description}");
            var roomthings = room.Things.Select(t => t.Actual);
            var roomentities = room.Entities.Select(t => t.Actual);
            Console.WriteLine(string.Join(", ", roomthings));
            Console.WriteLine(string.Join(", ", roomentities));
        }
    }
}
