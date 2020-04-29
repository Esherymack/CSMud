using System.Collections.Generic;
using System.Xml.Serialization;
using CSMud.Client;
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

        // The difference between static and takable is a matter of whether or not
        // the container can be put into a User's inventory
        // Examples of this would be satchels and bags; they can become part of a User's
        // equipment. Crates and chests might be non-static, but cannot be put into
        // a User's inventory.
        [XmlElement]
        public bool IsTakable { get; set; }

        [XmlElement]
        public int Weight { get; set; }

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

        [XmlIgnore]
        public int CurrentCapacity { get; set; }

        public Container()
        {
            Contents = new List<XMLReference<Item>>();

            IsUnlocked = true;
            IsStatic = false;
            IsTakable = false;
            Weight = 0;
            Capacity = 0;
            Name = "";
            Id = 0;
            Description = "";

            CurrentCapacity = 0;

            InitContainer();
        }

        public void InitContainer()
        {
            List<Item> DecodedContents = DecodeContents();

            foreach(Item i in DecodedContents)
            {
                CurrentCapacity += i.Weight;
            }

            if(CurrentCapacity > Capacity)
            {
                Capacity = CurrentCapacity;
            }
        }

        public void UseContainer(User sender)
        {
            string action = "";
            while(!CommandUtils.FuzzyEquals(action, "e"))
            {
                sender.Connection.SendMessage(@"What would you like to do?
t : take an item
a : add an item
p : pick up container
tc: take container
e : exit");
                action = sender.Connection.ReadMessage();
                if (CommandUtils.FuzzyEquals(action, "t"))
                {
                    TakeFromContainer(sender);
                }
                if(CommandUtils.FuzzyEquals(action, "a"))
                {
                    AddToContainer(sender);
                }
                if(CommandUtils.FuzzyEquals(action, "p"))
                {
                    PickUpContainer(sender);
                }
                if(CommandUtils.FuzzyEquals(action, "tc"))
                {
                    TakeContainer(sender);
                }
                if (CommandUtils.FuzzyEquals(action, "e"))
                {
                    return;
                }
            }
        }

        public void TakeFromContainer(User sender)
        {
            List<Item> dc = DecodeContents();
            if (Contents.Count > 0)
            {
                sender.Connection.SendMessage("The container has: ");
                foreach (Item i in dc)
                {
                    sender.Connection.SendMessage($"{i.Name}");
                }
                sender.Connection.SendMessage("What would you like to take?");
                string takeItem = sender.Connection.ReadMessage();
                var item = Contents.Find(t => CommandUtils.FuzzyEquals(t.Actual.Name, takeItem));

                if (item == null)
                {
                    sender.Connection.SendMessage($"There is no {item.Actual.Name} in the container.");
                }

                else
                {
                    sender.Inventory.AddToInventory(item.Actual);
                    sender.Connection.SendMessage($"The {item.Actual.Name} has been added to your inventory.");
                    CurrentCapacity -= item.Actual.Weight;
                    Contents.Remove(item);
                }
            }
            else
            {
                sender.Connection.SendMessage($"The {Name} is empty.");
            }

            if(CurrentCapacity < 0)
            {
                CurrentCapacity = 0;
            }
        }

        public void AddToContainer(User sender)
        {
            if(CurrentCapacity <= Capacity)
            {
                sender.Connection.SendMessage("You have: ");
                foreach (Item i in sender.Inventory.Items)
                {
                    sender.Connection.SendMessage($"{i.Name} - Weight: {i.Weight}");
                }
                sender.Connection.SendMessage($"The {Name} can hold {Capacity - CurrentCapacity} more units.");

                sender.Connection.SendMessage("What would you like to put in the container?");
                string addItem = sender.Connection.ReadMessage();
                Item item = sender.Inventory.Items.Find(t => CommandUtils.FuzzyEquals(t.Name, addItem));

                if(item.Weight + CurrentCapacity > Capacity)
                {
                    sender.Connection.SendMessage($"The {Name} cannot hold that!");
                }
                else
                {
                    sender.Inventory.RemoveFromInventory(item);
                    CurrentCapacity += item.Weight;

                    XMLReference<Item> xmlItem = new XMLReference<Item> { Actual = item };

                    Contents.Add(xmlItem);
                    
                    sender.Connection.SendMessage($"The {item.Name} has been added to the {Name}.");

                    return;
                }
            }
            else
            {
                sender.Connection.SendMessage($"The {Name} is full.");
                return;
            }
        }

        public void PickUpContainer(User sender)
        {

        }

        public void TakeContainer(User sender)
        {

        }

        public List<Item> DecodeContents()
        {
            List<Item> DecodedContents = new List<Item>();

            foreach (var i in Contents)
            {
                DecodedContents.Add(i.Actual);
            }

            return DecodedContents;
        }



        public override string ToString()
        {
            return $"{Name}";
        }

    }
}
