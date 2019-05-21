using System.Collections.Generic;
using System.Linq;
using CSMud.Client;
using CSMud.Thingamajig;
using CSMud.Utils;

/* Combat class for CSMud
 * Combat is initiated by interacting with enemies through 'attack' or 'talk' commands.
 */

namespace CSMud.Events
{
    public class Combat
    {
        // The list of users currently in the combat session.
        public List<User> Combatants { get; set; }
        // The enemy of the fight
        public Entity Target { get; set; }

        // The enemy's next strike
        public int Strike { get; set; }
        // The enemy's next block
        public int EnemyBlock { get; set; }
        // The enemy's target
        public User AttackTarget { get; set; }

        // Flavor text for enemy actions
        public string AttackSuccessFlavor { get; set; }
        public string AttackFailFlavor { get; set; }
        public string AttackCriticalFlavor { get; set; }
        public string AttackWeakFlavor { get; set; }
        public string BlockFlavor { get; set; }
        public string LowHealthFlavor { get; set; }

        public Combat(Entity target)
        {
            Combatants = new List<User>();
            Target = target;
            Strike = Target.Damage;
            EnemyBlock = Target.Defense;
            GetFlavorText();
        }

        public void GetFlavorText()
        {
            if (CommandUtils.FuzzyEquals(Target.AttackSpeed, "fast"))
            {
                AttackSuccessFlavor = $"The {Target.Name} attacks quickly!";
                AttackFailFlavor = $"The {Target.Name} tries to strike fast, but misses!";
                AttackCriticalFlavor = $"The {Target.Name} strikes with brutal speed!";
                AttackWeakFlavor = $"The {Target.Name} just nicks you!";
                BlockFlavor = $"The {Target.Name} blocks your attack!";
                LowHealthFlavor = $"The {Target.Name} looks weak!";
                return;
            }
            if (CommandUtils.FuzzyEquals(Target.AttackSpeed, "slow"))
            {
                AttackSuccessFlavor = $"The {Target.Name} attacks slowly!";
                AttackFailFlavor = $"The {Target.Name} tries to strike, but is too slow!";
                AttackCriticalFlavor = $"The {Target.Name} slams you into the ground!";
                AttackWeakFlavor = $"The {Target.Name} barely hits you!";
                BlockFlavor = $"The {Target.Name} casually blocks your attack!";
                LowHealthFlavor = $"The {Target.Name} looks like it might pass out!";
                return;
            }
            if (CommandUtils.FuzzyEquals(Target.AttackSpeed, "mid"))
            {
                AttackSuccessFlavor = $"The {Target.Name} attacks!";
                AttackFailFlavor = $"The {Target.Name} tries to strike, but misses!";
                AttackCriticalFlavor = $"The {Target.Name} lands a brutal blow!";
                AttackWeakFlavor = $"The {Target.Name} strikes weakly!";
                BlockFlavor = $"The {Target.Name} blocks your attack!";
                LowHealthFlavor = $"The {Target.Name} looks weak!";
                return;
            }
        }

        public void CombatSay(string msg)
        {
            lock (Combatants)
            {
                foreach (User user in Combatants)
                {
                    user.Connection.SendMessage(msg);
                }
            }
        }

        public void EnemyTurn()
        {
            var highestPresence = Combatants.Max(u => u.Player.Stats.Presence);
            AttackTarget = Combatants.Where(u => u.Player.Stats.Presence == highestPresence).First();
            if(AttackTarget == null)
            {
                Target.Combat = null;
                return;
            }
            if (AttackTarget.Player.Stats.CurrHealth <= 0)
            {
                Target.Combat = null;
                return;
            }
            // Roll to see if the monster lands a hit
            int hit = Dice.RollHundred();
            int attack = Strike;
            if (hit >= AttackTarget.Player.Stats.Agility)
            {
                // Check if the attack is a critical
                int crit = Dice.RollHundred();
                if(crit >= AttackTarget.Player.Stats.CritAvoid)
                {
                    AttackTarget.Connection.SendMessage(AttackCriticalFlavor);
                    attack = attack + (Strike / 2);
                }
                if(crit < AttackTarget.Player.Stats.CritAvoid)
                {
                    // If it's not critical, check if it's weak
                    int weak = Dice.RollHundred();
                    if(weak <= AttackTarget.Player.Stats.Luck)
                    {
                        AttackTarget.Connection.SendMessage(AttackWeakFlavor);
                        attack = attack - (Strike / 2);
                    }
                    AttackTarget.Connection.SendMessage(AttackSuccessFlavor);
                }
                // If the player is blocking, attack gets a further strike to damage
                if(AttackTarget.Player.IsBlocking)
                {
                    attack = attack - AttackTarget.Player.Stats.Defense;
                    if(attack < 0)
                    {
                        attack = 0;
                    }
                    CombatSay($"{Target.Name} is blocked by {AttackTarget.Name}!");
                }
                AttackTarget.Player.TakeDamage(attack);
                CombatSay($"{Target.Name} deals {Strike} damage to {AttackTarget.Name}");
                AttackTarget.Connection.SendMessage($"Your current health is {AttackTarget.Player.Stats.CurrHealth}");
                return;
            }
            AttackTarget.Connection.SendMessage(AttackFailFlavor);
            AttackTarget.Connection.SendMessage($"Your current health is {AttackTarget.Player.Stats.CurrHealth}");
        }

        public void Attack(User current)
        {
            // first roll:
            int hit = Dice.RollHundred();
            hit = hit + current.Player.Stats.Accuracy;
            // Check and see if the attack actually hits
            if(hit < Target.MinStrike)
            {
                current.Connection.SendMessage("Miss!");
                return;
            }
            int damage = current.Player.Stats.Damage;
            // if the attack does hit, determine if the attack is a critical hit.
            int crit = Dice.RollHundred();
            crit = crit + (current.Player.Stats.Luck);
            // A critical hit does base damage + 1/2 the base damage value.
            if(crit >= Target.CritChance)
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
                        if(CommandUtils.FuzzyEquals(thing.WeaponType, "slow"))
                        {
                            damage = damage + (current.Player.Stats.Strength / 2);
                            current.Connection.SendMessage($"Bonus: Slow Weapon - {damage} damage!");
                        }
                        if(CommandUtils.FuzzyEquals(thing.WeaponType, "fast"))
                        {
                            damage = damage + (current.Player.Stats.Dexterity / 2);
                            current.Connection.SendMessage($"Bonus: Fast Weapon - {damage} damage!");
                        }
                        if(CommandUtils.FuzzyEquals(thing.WeaponType, "spell"))
                        {
                            damage = damage + (current.Player.Stats.Knowledge / 2);
                            current.Connection.SendMessage($"Bonus: Magic - {damage} damage!");
                        }
                        if(CommandUtils.FuzzyEquals(thing.WeaponType, "ranged"))
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
            damage = damage - (Target.Defense);
            if(damage < 0)
            {
                damage = 0;
            }
            // and deal damage:
            Target.Health = Target.Health - damage;
            CombatSay($"{current.Name} dealt {damage} to {Target.Name}!");
            if (Target.Health <= 0)
            {
                Target.IsDead = true;
                CombatSay($"{Target.Name} has been defeated by {current.Name}!");
                return;
            }
            foreach (User user in Combatants)
            {
                user.Connection.SendMessage($"{Target.Name} has {Target.Health} health left");
            }
         }
        public void Defend(User current)
        {
            CombatSay($"{current.Name} defends!");
            // Roll to see if the defend is successful
            int defend = Dice.RollHundred();
            // Defend chance is modded by luck and defense level
            defend = defend + current.Player.Stats.Defense + current.Player.Stats.Luck;
            // Check and see if defend is >= target's min strike defense
            if (defend < Target.MinDefend)
            {
                current.Connection.SendMessage("Defense failed!");
                return;
            }
            current.Player.IsBlocking = true;
        }

        public void Run(User current, Door run)
        {
            if(Combatants.Count > 1)
            {
                current.Connection.SendMessage("You cannot abandon your allies!");
                return;
            }
            if(run == null)
            {
                current.Connection.SendMessage("There is no escape!");
                return;
            }
            current.Connection.SendMessage($"You manage to escape {run.Direction}!");
            Combatants.Remove(current);
            current.Player.Combat = null;
            current.CurrRoomId = run.RoomsIConnect[1];
        }
    }
}
