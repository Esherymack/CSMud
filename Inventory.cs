using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Inventory class for CSMud
 * Every User has a unique Inventory
 * An Inventory is a collection of Things 
 */

namespace CSMud
{ 
    public class Inventory
    {
        public List<Thing> Things { get; set; }

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
