﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

/* MapBuild generates the list of Thing objects, the list of Entity objects,
 * and the list of Room objects that implement those Thing and Entitiy objects,
 * effectively building the interactable map.
 */

namespace CSMud
{
    public class MapBuild
    { 
        public List<Thing> Things { get; set; }
        public List<Entity> Entities { get; set; }
        public List<Room> Rooms { get; set; }
        public Dictionary<int, Thing> AllThings { get; }

        public MapBuild()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            Console.WriteLine("Generating map...");
            Console.WriteLine("\n");
            Things = CreateThing(); 
            Entities = CreateEntity();
            CreateRoom();
            Console.WriteLine("\n");
            Console.WriteLine("Map generated!");
        }

        /* The process with all of these functions is basically the same.
         */      
        public List<Thing> CreateThing()
        {
            // Define a new serializer object 
            XmlSerializer serializer = new XmlSerializer(typeof(List<Thing>), new XmlRootAttribute("Things"));
            // Read the data
            using (XmlReader reader = XmlReader.Create(@"..\..\data\thing.xml"))
            {
                // Set the list
                Things = (List<Thing>)serializer.Deserialize(reader);
                return Things;
            }
        }

        public List<Entity> CreateEntity()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entity>), new XmlRootAttribute("Entities"));
            using (XmlReader reader = XmlReader.Create(@"..\..\data\entity.xml"))
            {
                Entities = (List<Entity>)serializer.Deserialize(reader);
                return Entities;
            }
        }

        /* CreateRoom is a little different in that it calls upon an XMLReference object
         * kudos to my friend Matthew Hatch for coming up with that idea
         * basically he suggested that I "smarten" C#'s deserializer in this regard
         * I approached these XML files thinking about it like a SQL database, 
         * XMLReference objects basically act as INNER JOIN
         */       
        public void CreateRoom()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Room>), new XmlRootAttribute("Rooms"));
            using (XmlReader reader = XmlReader.Create(@"..\..\data\room.xml"))
            {
                Rooms = (List<Room>)serializer.Deserialize(reader);
                // Rooms are generated with specific Thing and Entity ID's
                // We can parse through them with XMLReference lists.
                Rooms.ForEach(i => XMLReference<Thing>.Link(i.Things, Things));
                Rooms.ForEach(i => XMLReference<Entity>.Link(i.Entities, Entities));
                // Printing is just for testing purposes, presently
                Rooms.ForEach(r => PrintRoomDescription(r));
            }
        }

        public static Dictionary<int, Thing> SetAllThings(List<Thing> things)
        {
            Dictionary<int, Thing> AllThings = things.ToDictionary(t => t.Id, t => t);
            return AllThings;
        }

        /* Just for testing purposes
         */
        void PrintRoomDescription(Room room)
        {
            Console.WriteLine($"{room.Doors.Aggregate((a, b) => $"{a}, {b}")}\t{room.Name}\t{room.Description}");
            var roomthings = room.Things.Select(t => t.Actual);
            var roomentities = room.Entities.Select(t => t.Actual);
            Console.WriteLine(string.Join("", roomthings));
            Console.WriteLine(string.Join("", roomentities));
        }
    }
}
