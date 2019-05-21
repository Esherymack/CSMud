using System;
using System.Collections.Generic;
using System.Linq;
using CSMud.Client;
using CSMud.Thingamajig;
using CSMud.Utils;

namespace CSMud.Events
{
    // Class for processing unparameterized commands
    public class ProcessUnparameterizedCommand
    {

        public MapBuild Map { get; }
        public List<User> Users { get; }

        public ProcessUnparameterizedCommand(MapBuild wm, List<User> users)
        {
            Map = wm;
            Users = users;
        }

        public void HandleHelpEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage(@"Help:
'help' : Display this message.
'quit' : Exit the game.
'look' : Look at the room you are in.
'who' : Look at the players in your current room.
'inventory' or 'i' : Display inventory.
'take <object>' : Take an item.
'hold <object>' : Hold an item in your inventory.
'drop <object>' : Drop a held or taken item.
'eat <object>' : Eat a food item.
'drink <object>' : Drink a drink or potion.
'examine <object>' : Examine an item.
'examine self' : Look at yourself.
'go <direction>' : Move between rooms through valid doors.
'no' or 'n' : Decline.
'yes' or 'y' : Agree.
'say <message>' : Talk to the players in your room.
'whisper <user> <message>' : Talk to a specific player. You cannot talk privately to Someones.
'talk <entity>' : Talk to an NPC.
'attack <entity>' : Attack an enemy.");
        }
   
        // Look gets the description of a room.
        public void HandleLookEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            int index = CommandUtils.GetCurrentRoomId(s, Map);
            s.Connection.SendMessage($"You look around:\n{Map.Rooms[index].Description}");
            if (CommandUtils.HasThings(s, Map))
            {
                s.Connection.SendMessage($"You see some interesting things: {string.Join(", ", Map.Rooms[index].Things.Select(t => t.Actual))}.");
            }
            if (CommandUtils.HasDoors(s, Map))
            {
                s.Connection.SendMessage($"You see doors to: {string.Join(", ", Map.Rooms[index].Doors.Select(t => t.Actual))}.");
            }
            if (Map.Rooms[index].DeadEntities.ToList().Count > 0)
            {
                s.Connection.SendMessage($"There are some dead bodies here: {string.Join(", ", Map.Rooms[index].DeadEntities.Select(t => t.Actual.Name))}");
            }
        }

        // Who gets a list of players currently in the sender's room.
        public void HandleWhoEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            List<User> currentUsers = new List<User>();
            lock (Users)
            {
                foreach (User user in Users)
                {
                    if (CommandUtils.GetCurrentRoomId(user, Map) == CommandUtils.GetCurrentRoomId(s, Map))
                    {
                        currentUsers.Add(user);
                    }
                }
                currentUsers.Remove(s);
            }
                        // if there is one player
            if (currentUsers.Count == 1)
            {
                s.Connection.SendMessage($"You see your ally, {string.Join(", ", currentUsers.Select(t => t.Name))}.");
            }
            // if there are multiple players
            else if (currentUsers.Count > 1)
            {
                s.Connection.SendMessage($"You see your allies, {string.Join(", ", currentUsers.Select(t => t.Name))}.");
            }
            // if there are no other players
            else
            {
                s.Connection.SendMessage("You have no allies nearby.");
            }
            if (CommandUtils.HasEntities(s, Map))
            {
                List<Entity> friendlies = new List<Entity>();
                List<Entity> meanies = new List<Entity>();
                List<Entity> sneakies = new List<Entity>();
                List<Entity> detectedSneakies = new List<Entity>();
                foreach (var i in Map.Rooms[CommandUtils.GetCurrentRoomId(s, Map)].Entities)
                {
                    // each i is a reference to an entity
                    if (CommandUtils.FuzzyEquals(i.Actual.Faction, "enemy"))
                    {
                        if (i.Actual.IsHidden)
                        {
                            sneakies.Add(i.Actual);
                        }
                        else
                        {
                            meanies.Add(i.Actual);
                        }
                    }
                    else 
                    {
                        friendlies.Add(i.Actual);
                    }                  
                }
                if (friendlies.Count > 0)
                {
                    s.Connection.SendMessage($"You can see: {string.Join(", ", friendlies.Select(t => t.Name))}");
                }
                if (meanies.Count > 0)
                {
                    s.Connection.SendMessage($"You can see some enemies: {string.Join(", ", meanies.Select(t => t.Name))}");
                }
                if (sneakies.Count > 0)
                {
                    // sender.Player.Stats.Dexterity >= directionGone.Actual.minDexterity
                    foreach(Entity entity in sneakies)
                    {
                        if (s.Player.Stats.Perception >= entity.minPerception)
                        {
                            detectedSneakies.Add(entity);
                        }
                    }
                    if(detectedSneakies.Count > 0)
                    {
                        s.Connection.SendMessage($"Although they are trying to be stealthy, you can see some enemies lurking in the shadows: {string.Join(", ", detectedSneakies.Select(t => t.Name))}");
                        return;
                    }
                    s.Connection.SendMessage("You don't see anyone new, but you sense a strange presence.");
                    return;
                }
            }
        }

        // Inventory gets the user's inventory.
        public void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            s.Connection.SendMessage($"Your current carry capacity is {s.Inventory.CurrentCapacity}/{s.Inventory.CarryCapacity}");
            if (s.Inventory.Empty)
            {
                s.Connection.SendMessage($"You turn out your pockets. You have nothing.");
            }
            else
            {
                s.Connection.SendMessage($"Your inventory consists of:\n{s.Inventory.ToString()}");
            }
        }

        // No says no.
        public void HandleNoEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You firmly shake your head no.");
        }

        // Yes says yes.
        public void HandleYesEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You nod affirmatively.");
        }
    }
}
