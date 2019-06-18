using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CSMud.Entity;

/* MapBuild generates the list of Item objects, the list of NPC objects,
 * and the list of Room objects that implement those Item and Entitiy objects,
 * effectively building the interactable map.
 */

namespace CSMud.Utils
{
    public class MapBuild
    {
        public List<Item> Items { get; set; }
        public List<NPC> NPCs { get; set; }
        public List<NPC> DeadNPCs { get; set; }
        public List<Door> Doors { get; set; }  
        public List<Room> Rooms { get; set; }
        public Dictionary<int, Item> AllItems { get; set; }

        public MapBuild()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            Console.WriteLine("Generating map...");
            Items = CreateItem();
            NPCs = CreateNPC();
            Doors = CreateDoor();
            CreateRoom();
            Console.WriteLine("Map generated!");
        }

        /* The process with all of these functions is basically the same.
         */
        public List<Item> CreateItem()
        {
            // Define a new serializer object
            XmlSerializer serializer = new XmlSerializer(typeof(List<Item>), new XmlRootAttribute("Items"));
            // Read the data
            using (XmlReader reader = XmlReader.Create(@"..\..\data\item.xml"))
            {
                // Set the list
                Items = (List<Item>)serializer.Deserialize(reader);
                foreach(Item items in Items)
                {
                    items.StatIncrease = items.StatIncreaseList.ToDictionary(t => t.Stat, t => t.Value);
                }
                return Items;
            }
        }

        public List<NPC> CreateNPC()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<NPC>), new XmlRootAttribute("NPCs"));
            using (XmlReader reader = XmlReader.Create(@"..\..\data\npc.xml"))
            {
                NPCs = (List<NPC>)serializer.Deserialize(reader);
                NPCs.ForEach(i => XMLReference<Item>.Link(i.Items, Items));
                return NPCs;
            }
        }

        public List<Door> CreateDoor()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Door>), new XmlRootAttribute("Doors"));
            using (XmlReader reader = XmlReader.Create(@"..\..\data\door.xml"))
            {
                Doors = (List<Door>)serializer.Deserialize(reader);
                return Doors;
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
                // Rooms are generated with specific Item and NPC ID's
                // We can parse through them with XMLReference lists.
                Rooms.ForEach(i => XMLReference<Item>.Link(i.Items, Items));
                Rooms.ForEach(i => XMLReference<NPC>.Link(i.NPCs, NPCs));
                Rooms.ForEach(i => XMLReference<Door>.Link(i.Doors, Doors));
                // Printing is just for testing purposes, presently
                // Rooms.ForEach(r => PrintRoomDescription(r));
            }
        }

        /* Just for testing purposes - prints various Item pools so that I can determine what's where */
        public static void PrintRoomDescription(Room room)
        {
            // Console.WriteLine($"{room.Doors.Aggregate((a, b) => $"{a}, {b}")}\t{room.Name}\t{room.Description}\t{room.Id}");
            var roomthings = room.Items.Select(t => t.Actual);
            var roomentities = room.NPCs.Select(t => t.Actual);
            // Console.WriteLine(string.Join("", roomthings));
            // Console.WriteLine(string.Join("", roomentities));
        }
    }
}
