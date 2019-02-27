using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMud
{
    public class Objects
    {
        // necessary?
        public Inventory Inventory { get; set; }
        public User User { get; }

        public Objects(User user, Inventory inventory)
        {
            this.Inventory = inventory;
            this.User = user;
        }

        // Equipables inherits from Takeables
        public void Equipables()
        {

        }

        // Consumables inherits from Takeables
        public void Consumables()
        {

        }

        // Takeables inherits from Moveables
        public void Takeables()
        {

        }

        // Moveables inherit from Interactables
        public void Moveables()
        {

        }

        // Interactables are every object
        public void Interactables()
        {

        }

        public void placeInInventory()
        {

        }

        public void placeInRoom()
        {

        }
    }
}
