using System.Collections.Generic;
using System.Xml.Serialization;
using CSMud.Utils;

/* A 'Container' is a unique entity that holds items. */

namespace CSMud.Entity
{
    [XmlRoot("Containers")]
    public class Container : Identifiable
    {
        [XmlElement]
        public List<XMLReference<Item>> Contents { get; set; }

        [XmlElement]
        public bool IsUnlocked { get; set; }

        // If a Container is static, it cannot be picked up and moved.
        [XmlElement]
        public bool IsStatic { get; set; }

        // Regardless of whether or not a container is static, it has a capacity.
        // However, if it is not static and a player is holding it,
        // they can increase their capactiy.
        [XmlElement]
        public int Capacity { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public int Id { get; set; }

        [XmlElement]
        public string Description { get; set; }

        public Container()
        {
            Contents = new List<XMLReference<Item>>();

            IsUnlocked = true;
            IsStatic = false;
            Capacity = 0;
            Name = "";
            Id = 0;
            Description = "";
        }

        public override string ToString()
        {
            return $"{Name}";
        }

    }
}
