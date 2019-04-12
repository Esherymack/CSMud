﻿using System;
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
        // The combat session ID
        // Using a ulong gives us roughly 18 quintillion available combat sessions
        // if we run out, something is seriously wrong.
        static ulong next_id = 0;
        public ulong CombatSession { get; set; }
        // The list of users currently in the combat session.
        public List<User> Combatants { get; set; }
        // The target of the fight
        public Entity Target { get; set; }

        public Combat(Entity target)
        {
            Target = target;
        }

        public void StartNewFight()
        {
            CombatSession = next_id++;
            // Set the target's CombatId = to the combat session
            Target.CombatId = CombatSession;
        }

        // Orders the players in the current Combat roster
        public void PlayerOrder()
        {
            Combatants = Combatants.OrderBy(o => o.Player.Stats.Presence).ToList();
        }

        public void Attack(User current)
        {
            // 1 AP is deducted for attacking, regardless of whether or not it hits.
            current.Player.AdjustAPDown(1);
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
            crit = crit + (current.Player.Stats.Luck / 2);
            // A critical hit does base damage + 1/2 the base damage value.
            if(crit >= 15)
            {
                damage = damage + (damage / 2);
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
            if(current.Player.Held != null)
            {
                foreach (Thing thing in current.Player.Held)
                {
                    if(thing.IsWeapon)
                    {
                        if(World.FuzzyEquals(thing.WeaponType, "slow"))
                        {
                            damage = damage + (current.Player.Stats.Strength / 2);
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "fast"))
                        {
                            damage = damage + (current.Player.Stats.Dexterity / 2);
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "spell"))
                        {
                            damage = damage + (current.Player.Stats.Knowledge / 2);
                        }
                        if(World.FuzzyEquals(thing.WeaponType, "ranged"))
                        {
                            damage = damage + (current.Player.Stats.Dexterity / 2) + (current.Player.Stats.Agility / 2);
                        }
                    }
                }
            }
            // otherwise assume that the player is not holding a weapon
            else
            {
                damage = damage + current.Player.Stats.Strength;
            }
            // Finally, consider enemy's defense level:
            damage = damage - (Target.Defense / 2);
            // and deal damage:
            Target.Health = Target.Health - damage;
            current.Connection.SendMessage($"Dealt {damage} damage to {Target.Name}!");
            current.Connection.SendMessage($"{Target.Name} has {Target.Health} health left");
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
