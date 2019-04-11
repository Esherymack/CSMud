using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Combat class for CSMud
 * Combat is initiated by interacting with enemies through 'attack' or 'talk' commands.
 */

namespace CSMud
{
    class Combat
    {
        static ulong next_id = 0;
        // The combat session ID
        public ulong CombatSession { get; set; }
        // The list of users currently in the combat session.
        public List<User> Combatants { get; set; }

        public Combat()
        {
            CombatSession = next_id++;
        }

        public void PlayerOrder()
        {

        }

        public void Attack()
        {

        }
        public void Defend()
        {
    
        }
        public void Heal()
        {

        }
        public void Run()
        {
    
        }
        public void Examine()
        {

        }


    }
}
