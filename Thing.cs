using System.Collections.Generic;
using System.Xml.Serialization;

/* A 'Thing' object is a collection of commands, an Id, a Name, and a Description
 */

namespace CSMud
{
    [XmlRoot("Things")]
    public class Thing : Identifiable
    {
        [XmlElement]
        public List<string> Commands { get; set; }
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Description { get; set; }
        // If an item is equippable
        [XmlElement]
        public bool IsWearable { get; set; }
        // If an item is wearable, then it has a 'slot'
        [XmlElement]
        public string Slot { get; set; }
        // A wearable must increase some stat(s) by some amount.
        [XmlElement]
        public List<string> StatIncrease { get; set; }
        // If an item is a weapon
        [XmlElement]
        public bool IsWeapon { get; set; }
        // Weapons deal damage.
        [XmlElement]
        public int Damage { get; set; }
        // Weight is directly involved in the player's Strength rating.
        [XmlElement]
        public int Weight { get; set; }

        public Thing()
        {
            Commands = new List<string>();
            Id = 0;
            Name = "";
            Description = "";
            IsWearable = false;
            Slot = "";
            StatIncrease = new List<string>();
            IsWeapon = false;
            Damage = 0;
            Weight = 0;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
