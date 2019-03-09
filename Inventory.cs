using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMud
{
    public class Inventory
    {
        // An inventory is a dictionary of ints and strings
        // The integer is the object number
        // The string is the object name + description
        // This data comes from a resource file

        // define params
        public List<Thing> Things { get; }

        // Constructor
        public Inventory(List<Thing> things)
        {
            this.Things = things;
        }

        public void addToInventory()
        {

        }

        public void removeFromInventory()
        {

        }

        public void listInventory()
        {

        }
    }
}
