using System.Collections.Generic;
using System.Linq;
using CSMud.Client;
using CSMud.Entity;
using CSMud.Utils;

namespace CSMud.Events
{
    public class ProcessParameterizedCommand
    {
        public MapBuild Map { get; set; }
        public List<User> Users { get; set; }

        public ProcessParameterizedCommand(MapBuild wm, List<User> users)
        {
            Map = wm;
            Users = users;
        }

        // Switch statement for handling events with params
        public void HandleParameterizedEvent(object sender, ParameterizedEvent e)
        {
            User s = sender as User;
            string action = e.Action;
            switch (e.Command)
            {
                case "say":
                    RoomSay($"{User.FormatMessage(action, s.Name)}", s);
                    break;
                case "whisper":
                    HandleWhisper(s, action);
                    break;
                case "take":
                    HandleTake(s, action);
                    break;
                case "drop":
                    HandleDrop(s, action);
                    break;
                case "eat":
                    HandleEat(s, action);
                    break;
                case "drink":
                    HandleDrink(s, action);
                    break;
                case "examine":
                    HandleExamine(s, action);
                    break;
                case "go":
                    HandleGo(s, action);
                    break;
                case "equip":
                case "wear":
                    HandleEquip(s, action);
                    break;
                case "remove":
                    HandleRemove(s, action);
                    break;
                case "hold":
                    HandleHold(s, action);
                    break;
                case "attack":
                    HandleAttack(s, action);
                    break;
                case "loot":
                    HandleLoot(s, action);
                    break;
                case "talk":
                    HandleTalkTo(s, action);
                    break;
                default:
                    s.Connection.SendMessage("You cannot do that.");
                    break;
            }
        }

        // RoomSay handles sending a message to all Users within a Room 
        // Semantically this is the same as Broadcast, but with the exception that it only sends to players whose CurrRoomId's match the speaker's.
        public void RoomSay(string msg, User sender)
        {
            lock (Users)
            {
                foreach (User user in Users)
                {
                    if (CommandUtils.GetCurrentRoomId(user, Map) == CommandUtils.GetCurrentRoomId(sender, Map))
                    {
                        user.Connection.SendMessage(msg);
                    }
                }
            }
        }
        
        // Send a message to a specific player
        void HandleWhisper(User sender, string msg)
        {
            string[] sl = CommandUtils.splitLine(msg);
            User recipient = CommandUtils.GetRecipient(sl[0], Users);
            if (recipient == null)
            {
                sender.Connection.SendMessage("You cannot talk to that person.");
                return;
            }
            if (recipient.Name == "Someone")
            {
                sender.Connection.SendMessage("You cannot whisper to strangers.");
                return;
            }
            recipient.Connection.SendMessage($"{sender.Name} whispers, '{sl[1]}'");
        }

        /* HandleTake and HandleDrop are relatively similar functions that just perform the inverses of each other
         * HandleTake checks if the item a user wants to take exists in the context of the room they are in.
         * if item exists, remove it from the room's list of references and add it to the player's inventory list instead       
         */
        void HandleTake(User sender, string e)
        {
            if(sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int roomId = CommandUtils.GetCurrentRoomId(sender, Map);
            var target = Map.Rooms[roomId].Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e));
            
            if (target == null)
            {
                sender.Connection.SendMessage("No such object exists.");
                return;
            }
            
            if (!target.Actual.Commands.Contains("take"))
            {
                sender.Connection.SendMessage("You cannot take that.");
                return;
            }
            
            // Prevent the chance that two users try to take the same item at the same time.
            lock (Map.Rooms[roomId].Items)
            {
                if (Map.Rooms[roomId].Items.Remove(target))
                {
                    sender.Inventory.setCurrentRaisedCapacity(target.Actual.Weight);
                    if(sender.Inventory.CurrentCapacity <= sender.Inventory.CarryCapacity)
                    {
                        sender.Inventory.AddToInventory(target.Actual);
                        return;
                    }
                    sender.Connection.SendMessage("You're carrying too much to take that.");
                }
                else
                {
                    sender.Connection.SendMessage("That item is no longer there.");
                }
            }
        }

        /* Like Handletake, HandleDrop checks for an item's existence, but in this case it has to be in the 
         * requesting user's inventory.
         * if item exists, remove it from the user inventory's list of refs and add to the room's Items instead
         */
        void HandleDrop(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int roomId = CommandUtils.GetCurrentRoomId(sender, Map);
            Item target = sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                HandleHoldDrop(sender, e);
                return;
            }
            
            if (!target.Commands.Contains("drop"))
            {
                sender.Connection.SendMessage("You cannot drop that.");
                return;
            }

             XMLReference<Item> item = new XMLReference<Item> { Actual = target };
             sender.Player.Drop(target);
             sender.Inventory.setCurrentLoweredCapacity(target.Weight);
             sender.Inventory.RemoveFromInventory(target);
             Map.Rooms[roomId].Items.Add(item);
        }

        void HandleHoldDrop(User sender, string e)
        {
            int roomID = CommandUtils.GetCurrentRoomId(sender, Map);
            var target = sender.Player.Held.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                sender.Connection.SendMessage("You are not holding that.");
                return;
            }
            
            if (!target.Commands.Contains("drop"))
            {
                sender.Connection.SendMessage("You cannot drop that.");
                return;
            }
            sender.Player.Drop(target);
            sender.Inventory.setCurrentRaisedCapacity(target.Weight);
            CommandUtils.RemoveItemChangeStats(target, sender);
            sender.Inventory.AddToInventory(target);
        }

        void HandleEat(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            
            if(!target.IsConsumable)
            {
                sender.Connection.SendMessage("You cannot eat that.");
                return;
            }
            if(!CommandUtils.FuzzyEquals(target.ConsumableType, "food"))
            {
                sender.Connection.SendMessage("You cannot eat that.");
                return;
            }
            if(target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            sender.Inventory.RemoveFromInventory(target);
            CommandUtils.ChangeStats(target, sender);
            sender.Inventory.setCurrentLoweredCapacity(target.Weight);
            return;
        }
        void HandleDrink(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if(!target.IsConsumable)
            {
                sender.Connection.SendMessage("You cannot drink that.");
                return;
            }
            if(!CommandUtils.FuzzyEquals(target.ConsumableType, "drink"))
            {
                sender.Connection.SendMessage("You cannot drink that.");
                return;
            }
            if(target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            sender.Inventory.RemoveFromInventory(target);
            CommandUtils.ChangeStats(target, sender);
            sender.Inventory.setCurrentLoweredCapacity(target.Weight);
            return;
        }

        /* HandleExamine allows a user to examine an object or themselves
         * 'examine self' returns a description of the user's player npc, including what they're wearing and appearance.
         * 'examine <item>' returns the description of the item. The item can be either in the room or in the user's inventory.
         * If the item is in neither, there is no description.       
         */
        void HandleExamine(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            if (e == null)
            {
                sender.Connection.SendMessage("Examine what?");
                return;
            }

            if (CommandUtils.FuzzyEquals(e, "self"))
            {
                sender.Connection.SendMessage("You look yourself over.");
                if (sender.Player.Equipped.Count > 0)
                {
                    sender.Connection.SendMessage($"Equipped: {string.Join(", ", sender.Player.Equipped)}");
                }
                if (sender.Player.Held.Count > 0)
                {
                    sender.Connection.SendMessage($"Held: {string.Join(", ", sender.Player.Held)}");
                }
                sender.Connection.SendMessage($"Health: {sender.Player.Stats.CurrHealth}; Defense: {sender.Player.Stats.Defense}");
                return;
            }

            int roomId = CommandUtils.GetCurrentRoomId(sender, Map);
            Item item = Map.Rooms[roomId].Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e))?.Actual ?? sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if(item == null)
            {
                // If it's not a item, try and see if the target is an npc
                HandleNPCExamine(sender, e);
                return;
            }
            if(!item.Commands.Contains("examine"))
            {
                sender.Connection.SendMessage("You cannot examine that.");
                return;
            }

            sender.Connection.SendMessage($"You examine the {e}.");
            sender.Connection.SendMessage($"{item.Description}");
        }

        void HandleNPCExamine(User sender, string e)
        {
            int roomId = CommandUtils.GetCurrentRoomId(sender, Map);
            NPC npc = Map.Rooms[roomId].NPCs.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e))?.Actual;
            if(npc == null)
            {
                // If it's not an npc or a item, it's not examinable.
                sender.Connection.SendMessage("That does not exist here!");
                return;
            }
            if (!npc.Commands.Contains("examine"))
            {
                sender.Connection.SendMessage("You cannot examine that.");
                return;
            }
            sender.Connection.SendMessage($"You examine the {e}.");
            sender.Connection.SendMessage($"{npc.Description}");
        }

        void HandleEquip(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            bool isWearing = false;

            if(!target.IsWearable)
            {
                sender.Connection.SendMessage("You cannot wear that.");
                return;
            }

            if (target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            if (target.IsWearable)
            {

                if (sender.Player.Equipped.Count == 6)
                {
                    sender.Connection.SendMessage("You have no other room to wear that.");
                    return;
                }

                foreach (Item item in sender.Player.Equipped)
                {
                    if (target.Slot == item.Slot)
                    {
                        sender.Connection.SendMessage($"You are already wearing a {target.Slot} item.");
                        isWearing = true;
                        break;
                    }
                }

                if (!isWearing)
                {
                    sender.Inventory.RemoveFromInventory(target);
                    sender.Player.Equip(target);
                    CommandUtils.ChangeStats(target, sender);
                    sender.Inventory.setCurrentLoweredCapacity(target.Weight);
                    return;
                }
            }
        }

        void HandleRemove(User sender, string e)
        {
            var target = sender.Player.Equipped.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if (target == null)
            {
                sender.Connection.SendMessage("You are not wearing that.");
                return;
            }
            
            if(target.Commands.Contains("remove"))
            {
                sender.Player.Unequip(target);
                sender.Inventory.setCurrentRaisedCapacity(target.Weight);
                CommandUtils.RemoveItemChangeStats(target, sender);
                sender.Inventory.AddToInventory(target);
                return;
            }
           
            sender.Connection.SendMessage("You cannot remove that.");
        }

        void HandleHold(User sender, string e)
       {
            var target = sender.Inventory.Items.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            if (target.Commands.Contains("hold"))
            {
                if(sender.Player.Held.Count == 2)
                {
                    sender.Connection.SendMessage("You have no free hands.");
                    return;
                }

                sender.Inventory.RemoveFromInventory(target);
                sender.Inventory.setCurrentLoweredCapacity(target.Weight);
                CommandUtils.ChangeStats(target, sender);
                sender.Player.Hold(target);
                return;
            }
            
            sender.Connection.SendMessage("You cannot hold that.");
        }

        /* HandleGo handles moving between rooms.
         * TODO: Find a better way to handle the RoomsIConnect list - currently, in the XML, rooms must be 
         * placed in the order of CURRENT ROOM in index 0 and CONNECTING ROOM in index 1    
         * there's probably a better way to handle this situation.
         */
        void HandleGo(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            if (CommandUtils.FuzzyEquals(e, "north"))
            {
                e = "n";
            }
            if(CommandUtils.FuzzyEquals(e, "south"))
            {
                e = "s";
            }
            if(CommandUtils.FuzzyEquals(e, "east"))
            {
                e = "e";
            }
            if(CommandUtils.FuzzyEquals(e, "west"))
            {
                e = "w";
            }
            int currentRoomId = CommandUtils.GetCurrentRoomId(sender, Map);
            int numDoors = Map.Rooms[currentRoomId].Doors.Select(t => t.Actual).Count();
            var directionGone = Map.Rooms[currentRoomId].Doors.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Direction, e))?.Actual;
            if (numDoors == 0)
            {
                sender.Connection.SendMessage("There are no doors here. You cannot go anywhere.");
                return;
            }
                
            if(directionGone == null)
            {
                sender.Connection.SendMessage("There is no door there.");
                return;
            }
                
            void proceed()
            {
                sender.CurrRoomId = directionGone.RoomsIConnect[1];
            }
            
            if(!directionGone.Locked)
            {
                proceed();
                return;
            }
              
            if(sender.Player.Stats.Dexterity >= directionGone.minDexterity)
            {
                sender.Connection.SendMessage("Although the door is locked, you are deft enough to pick your way in.");
                proceed();
                directionGone.Locked = false;
                return;
            }
                
            if(directionGone.HasKey)
            {
                if(CommandUtils.OwnsItem(sender, "key"))
                {
                    proceed();
                    return;
                }
                sender.Connection.SendMessage("This door is locked and has a key. Look around!");
            }
            else
            {
                sender.Connection.SendMessage("You are unable to open the door.");
            }
        }

        // Attack an enemy
        void HandleAttack(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int currRoom = CommandUtils.GetCurrentRoomId(sender, Map);
            NPC target = CommandUtils.GetTarget(currRoom, e, Map);
            // Make sure the target is actually attackable
            if(target == null)
            {
                sender.Connection.SendMessage("You cannot attack that.");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "ally"))
            {
                sender.Connection.SendMessage("You cannot attack friendly people!");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "dead"))
            {
                sender.Connection.SendMessage("You cannot attack dead bodies!");
                return;
            }
            // If these checks pass, then there is an available target to fight
            // Check and see if the target is already engaged : if so, join the target session
          //  RoomSay($"{sender.Name} is attacking the {target.Name}!", sender);
            if (target.Combat == null)
            {
                target.Combat = new Combat(target);
            }
            target.Combat.Combatants.Add(sender);
            sender.Player.Combat = target.Combat;

            // Pre-emptively get the direction to run in in case the user decides to try to run.
            var runDir = Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].Doors.Where(d => !d.Actual.Locked).FirstOrDefault()?.Actual;

            // Combat loop: check turns            
            while(target.Combat != null && sender.Player.Combat != null)
            {
                sender.Player.IsBlocking = false;
                sender.Connection.SendMessage(@"a: Attack
d: Defend
h: Heal
r: Run");
                if (sender.Player.Stats.CurrHealth <= 0)
                {
                    break;
                }
                else
                {
                    string action = sender.Connection.ReadMessage();
                    if (CommandUtils.FuzzyEquals(action, "a"))
                    {
                        target.Combat.Attack(sender);
                    }
                    if (CommandUtils.FuzzyEquals(action, "d"))
                    {
                        target.Combat.Defend(sender);
                    }
                    if(CommandUtils.FuzzyEquals(action, "h"))
                    {
                        sender.Connection.SendMessage("Consume what?");
                        var consumables = sender.Inventory.Items.Where(t => t.IsConsumable);
                        foreach(Item item in consumables)
                        {
                            sender.Connection.SendMessage(item.Name);
                        }
                        string choice = sender.Connection.ReadMessage();
                        HandleEat(sender, choice);
                    }
                    if (CommandUtils.FuzzyEquals(action, "r"))
                    {
                        target.Combat.Run(sender, runDir);
                        target.Combat = null;
                        break;
                    }
                }
                if(target.Health <= 0)
                {
                    break;
                }
                target.Combat.EnemyTurn();
            }

            if (sender.Player.Stats.CurrHealth <= 0)
            {
                sender.Player.Combat = null;
                target.Combat = null;
                sender.Connection.SendMessage("You have been defeated!");
                if(Users.Count > 2)
                {
                    sender.Connection.SendMessage("Would you like to wait for a revive? (y/n)");
                    string action = sender.Connection.ReadMessage();
                    if(CommandUtils.FuzzyEquals(action, "y"))
                    {
                        sender.Player.IsDead = true;
                        return;
                    }
                }
                sender.Connection.SendMessage("As you were the only soul in this place, you have been sent back to the start.");
                foreach(Item items in sender.Inventory.Items.ToList())
                {
                    sender.Inventory.RemoveFromInventory(items);
                    XMLReference<Item> item = new XMLReference<Item> { Actual = items };
                    Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].Items.Add(item);
                }
                sender.Player.Stats.CurrHealth = sender.Player.Stats.MaxHealth;
                sender.CurrRoomId = 0001;
                return;
            }
            // After this loop, the target is dead. Remove them from the npc list and add them to the dead list/faction.
            target.Faction = "dead";

            // Upon death, an npc's inventory gets dropped to the room.
            /* foreach(Item items in target.Inventory.Items.ToList())
            {
                target.Inventory.RemoveFromInventory(items);
                XMLReference<Item> item = new XMLReference<Item> { Actual = items };
                Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].Items.Add(item);
            } */

            foreach (var i in Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].NPCs.ToList())
            {
                if(i.Actual.Name == target.Name)
                {
                    Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].NPCs.Remove(i);
                    Map.Rooms[CommandUtils.GetCurrentRoomId(sender, Map)].DeadNPCs.Add(i);
                }
            }

            sender.Connection.SendMessage("Send 'q' to exit combat.");
            sender.Player.Combat = null;
            target.Combat = null;
        }

        void HandleLoot(User sender, string e)
        {

        }

        // Talk to an NPC
        void HandleTalkTo(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int currRoom = CommandUtils.GetCurrentRoomId(sender, Map);
            NPC target = CommandUtils.GetTarget(currRoom, e, Map);
            if(target == null)
            {
                sender.Connection.SendMessage("You babble to no one in particular for a moment.");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "enemy"))
            {
                sender.Connection.SendMessage("While talking your way out of confrontation is admirable, it won't work in this situation.");
                HandleAttack(sender, e);
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "dead"))
            {
                sender.Connection.SendMessage("He's dead, Jim.");
                return;
            }
            target.Conversation = new Conversation(target, sender);
            sender.Player.Conversation = target.Conversation;

            target.Conversation.Greeting();

            while(target.Conversation != null && sender.Player.Conversation != null)
            {
                sender.Connection.SendMessage(@"w: Who
n: News
t: Trade
q: Quest
b: Bye");
                string action = sender.Connection.ReadMessage();
                if(CommandUtils.FuzzyEquals(action, "w"))
                {
                    target.Conversation.Who();
                }
                if(CommandUtils.FuzzyEquals(action, "n"))
                {
                    target.Conversation.News();
                    sender.Connection.SendMessage("News placeholder");
                }
                if(CommandUtils.FuzzyEquals(action, "t"))
                {
                    if(CommandUtils.FuzzyEquals(target.Faction, "ally") || CommandUtils.FuzzyEquals(target.Faction, "neutral"))
                    {
                        target.Conversation.Trade();
                    }
                    else
                    {
                        sender.Connection.SendMessage(target.Conversation.TradeFlavor);
                    }
                }
                if(CommandUtils.FuzzyEquals(action, "q"))
                {
                    if (CommandUtils.FuzzyEquals(target.Faction, "ally") || (CommandUtils.FuzzyEquals(target.Faction, "neutral")))
                    {
                        if(target.HasQuest)
                        {
                            target.Conversation.Quest();
                        }
                        else
                        {
                            sender.Connection.SendMessage($"{target.Name} does not have a quest for you.");
                        }
                    }
                    else 
                    {
                        sender.Connection.SendMessage(target.Conversation.QuestFlavor);
                    }
                }
                if(CommandUtils.FuzzyEquals(action, "b"))
                {
                    target.Conversation.Bye();
                    break;
                }
            }
            target.Conversation = null;
            sender.Player.Conversation = null;
        }
    }
}
