using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CSMud
{
    public class MapBuild
    {
        public void GenerateMap()
        {
            
        }

        public void CreateThing()
        {
            List<Thing> Things = new List<Thing>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Thing>));
            XmlReader reader = XmlReader.Create(@"data/thing.xml");
            Things = (List<Thing>)serializer.Deserialize(reader);
        }

        public void CreateEntity()
        {
            List<Entity> Entities = new List<Entity>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Entity>));
            XmlReader reader = XmlReader.Create(@"data/entity.xml");
            Entities = (List<Entity>)serializer.Deserialize(reader);
        }

        public void CreateRoom()
        {
            List<Room> Rooms = new List<Room>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Room>));
            XmlReader reader = XmlReader.Create(@"data/room.xml");
            Rooms = (List<Room>)serializer.Deserialize(reader);
        }
    }
}
