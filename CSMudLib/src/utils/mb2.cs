using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using CSMud.Entity;
using System.IO;

namespace CSMudLib.Utils
{
    public class mb2
    {
        public List<Item> Items { get; set; }
        public List<NPC> NPCs { get; set; }
        public List<NPC> DeadNPCs { get; set; }
        public List<Door> Doors { get; set; }
        public List<Room> Rooms { get; set; }
        public Dictionary<int, Item> AllItems { get; set; }

        public void LoadJson(string filename)
        {

            using (StreamReader r = new StreamReader(filename))
            {
                string json = r.ReadToEnd();
                
            }
        }

    }
}
