using System;
namespace CSMud
{
    public class EntityStats
    {
        // Health is health, defense is defense, etc.
        public int Health { get; set; }
        public int Defense { get; set; }
        // Entities deal implicit damage without exactly having to own weapons.
        public int Damage { get; set; }
        // Hidden entities have a minimum perception check to actually detect as entities.
        public int MinPerception { get; set; }
        // Entities also have a drop inventory - when they die, they drop it; friendlies can 'trade' with pc's.
        public Inventory EntityInventory { get; set; }

        public EntityStats()
        {
        }
    }
}
