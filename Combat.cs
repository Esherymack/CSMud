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
    public class Combat
    {
        // The list of users currently in the combat session.
        public List<User> Combatants { get; set; }
        // The target of the fight
        public Entity Target { get; set; }

        public Combat(Entity target)
        {
            Combatants = new List<User>();
            Target = target;
        }

        public void CombatSay(string msg, User sender)
        {
            lock(Combatants)
            {
                foreach(User user in Combatants)
                {
                    if(!World.FuzzyEquals(user.Name, sender.Name))
                    {
                        user.Connection.SendMessage(msg);
                    }
                }
            }
        }

        public void TargetAttackGo(Entity enemy)
        {

        }

        public void Attack(User current)
        {
            // first roll:
            int hit = Dice.RollTen();
            hit = hit + (current.Player.Stats.Luck / 10) + (current.Player.Stats.Accuracy / 2);
            // Check and see if the attack actually hits
            if(hit < Target.MinStrike)
            {
                current.Connection.SendMessage("Miss!");
                return;
            }
            int damage = current.Player.Stats.Damage;
            // if the attack does hit, determine if the attack is a critical hit.
            int crit = Dice.RollTwenty();
            crit = crit + (current.Player.Stats.Luck / 10);
            // A critical hit does base damage + 1/2 the base damage value.
            if(crit >= 18)
            {
                damage = damage + (damage / 2);
                current.Connection.SendMessage($"Critical hit!");
            }
            /* Next, determine attack bonuses.
               Attack bonuses come from multiple sources: 
                - Players who do not have a weapon equipped get a damage modifier based on strength
                - Players who have "fast" weapons such as daggers get a damage modifier based on dex
                - Players who have "slow" weapons such as swords get damage modifiers based on strength
                - Players who have "spell" weapons such as staves get damage modifiers based on knowledge
                - Players who have "ranged" weapons such as bows get damage modifiers based on dex and agility
                - Spell status : Some spells will buff damage
                - Consumable status : Some consumables have damage modifiers
                - Equipment bonus : Some equipment will modify damage
                However, because all of these factors directly change stats on impact, we just check stat bonuses here.
            */
            if(current.Player.Held.Count != 0)
            {
                foreach (Thing thing in current.Player.Held)
                {
                    if(thing.IsWeapon)
                    {
                        if(World.FuzzyEquals(thing.WeaponType, "slow"))
                        {
                            damage = damage + (current.Player.Stats.Strength / 2);
                            current.Connection.SendMessage($"Bonus: Slow Weapon - {damage} damage!");
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "fast"))
                        {
                            damage = damage + (current.Player.Stats.Dexterity / 2);
                            current.Connection.SendMessage($"Bonus: Fast Weapon - {damage} damage!");
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "spell"))
                        {
                            damage = damage + (current.Player.Stats.Knowledge / 2);
                            current.Connection.SendMessage($"Bonus: Magic - {damage} damage!");
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "ranged"))
                        {
                            damage = damage + (current.Player.Stats.Dexterity / 2) + (current.Player.Stats.Agility / 2);
                            current.Connection.SendMessage($"Bonus: Ranged - {damage} damage!");
                        }
                    }
                }
            }
            // otherwise assume that the player is not holding a weapon
            else
            {
                damage = damage + current.Player.Stats.Strength;
                current.Connection.SendMessage($"Bonus: Pugilist - {damage} damage!");
            }
            // Finally, consider enemy's defense level:
            damage = damage - (Target.Defense / 2);
            // and deal damage:
            Target.Health = Target.Health - damage;
            CombatSay($"{current.Name} dealt {damage} to {Target.Name}!", current);
            current.Connection.SendMessage($"Dealt {damage} damage to {Target.Name}!");
            if (Target.Health <= 0)
            {
                Target.IsDead = true;
                CombatSay($"{Target.Name} has been defeated by {current.Name}!", current);
                current.Connection.SendMessage($"Defeated the {Target.Name}!");
                return;
            }
            foreach (User user in Combatants)
            {
                user.Connection.SendMessage($"{Target.Name} has {Target.Health} health left");
            }
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
