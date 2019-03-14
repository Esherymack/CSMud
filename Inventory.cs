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
        public bool Empty => Things.Count == 0;
        
        public Inventory()
        {
            Things = new List<Thing>();
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
