using System.Collections.Generic;

/* Inventory class for CSMud
 * Every User has a unique Inventory
 * An Inventory is a collection of Things 
 */

namespace CSMud.Entity
{
    public class Inventory
    {
        public List<Item> Items { get; }
        public int CarryCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public bool Empty => Items.Count == 0;
        
        public Inventory()
        {
            Items = new List<Item>();
            CarryCapacity = 50;
            CurrentCapacity = 0;
        }

        public void setCarryCapacity(int strength)
        {
            CarryCapacity = CarryCapacity + strength;
        }

        public void setCurrentRaisedCapacity(int weight)
        {
            CurrentCapacity = CurrentCapacity + weight;
        }

        public void setCurrentLoweredCapacity(int weight)
        {
            CurrentCapacity = CurrentCapacity - weight;
            if(CurrentCapacity < 0)
            {
                CurrentCapacity = 0;
            }
        }

        public void AddToInventory(Item item)
        {              
            Items.Add(item);
        }

        public void RemoveFromInventory(Item item)
        {
            Items.Remove(item);
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Items)}";
        }
    }
}
