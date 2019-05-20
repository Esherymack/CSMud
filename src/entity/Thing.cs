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
        public List<StatValue> StatIncreaseList { get; set; }
        [XmlIgnore]
        public Dictionary<string, int> StatIncrease { get; set; }
        // If an item is consumable
        [XmlElement]
        public bool IsConsumable { get; set; }
        [XmlElement]
        public string ConsumableType { get; set; }
        // If an item is a weapon
        [XmlElement]
        public bool IsWeapon { get; set; }
        // Weight is directly involved in the player's Strength rating.
        [XmlElement]
        public int Weight { get; set; }
        // Value determines trade value 
        [XmlElement]
        public int Value { get; set; }
        [XmlElement]
        public string WeaponType { get; set; }

        public Thing()
        {
            Commands = new List<string>();
            Id = 0;
            Name = "";
            Description = "";
            IsWearable = false;
            Slot = "";
            StatIncreaseList = new List<StatValue>();
            IsWeapon = false;
            Weight = 0;
            Value = 0;
            WeaponType = "";
            ConsumableType = "";
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
