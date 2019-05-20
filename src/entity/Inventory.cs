using System.Collections.Generic;

/* Inventory class for CSMud
 * Every User has a unique Inventory
 * An Inventory is a collection of Things 
 */

namespace CSMud
{
    public class Inventory
    {
        public List<Thing> Things { get; }
        public int CarryCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public bool Empty => Things.Count == 0;
        
        public Inventory()
        {
            Things = new List<Thing>();
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

        public void AddToInventory(Thing thing)
        {              
            Things.Add(thing);
        }

        public void RemoveFromInventory(Thing thing)
        {
            Things.Remove(thing);
        }

        public override string ToString()
        {
            return $"{string.Join(", ", Things)}";
        }
    }
}
