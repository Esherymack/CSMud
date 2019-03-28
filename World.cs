using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace CSMud
{
    // A World holds connections and handles broadcasting messages from those connections
    // The world also handles taking care of all of the commands, both parameterized and unparameterized.
    // TODO: Reduce obscene amount of if/else/if statements
    public class World
    {
        // a world has connections
        public List<User> Users { get; }

        // a beat is a periodic message sent over the server
        private Timer Beat { get; }

        public MapBuild WorldMap { get; set; }

        // constructor
        public World()
        {
            // create a list object of connections
            Users = new List<User>();
            // Create a beat on the server
            Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            Beat.Elapsed += OnTimedEvent;

            // Generate the map last
            WorldMap = new MapBuild();
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Broadcast("Something scurries around in the distance.");
        }

        // add new Connections to the world
        public void NewConnection(Socket clientSock)
        {
            User handshake(Connection conn)
            {
                conn.SendMessage("Please enter a name: ");
                while (true)
                {
                    string username = conn.ReadMessage();
                    if (username is null)
                    {
                        continue;
                    }
                    if (username == "")
                    {
                        username = "Someone";
                    }
                    User user = new User(conn, this, username);
                    lock (Users)
                    {
                        Users.Add(user);
                    }

                    #region Subscribe the user to events. 
                    user.RaiseHelpEvent += HandleHelpEvent;
                    user.RaiseLookEvent += HandleLookEvent;
                    user.RaiseWhoEvent += HandleWhoEvent;
                    user.RaiseInventoryQueryEvent += HandleInventoryQueryEvent;
                    user.RaiseNoEvent += HandleNoEvent;
                    user.RaiseYesEvent += HandleYesEvent;
                    user.RaiseParameterizedEvent += HandleParameterizedEvent;
                    #endregion

                    return user;
                }
            }

            Task.Run(() =>
            {
                using (Connection conn = new Connection(clientSock))
                {
                    User user = null;

                    try
                    {
                        user = handshake(conn);

                        // tell the server that a user has connected
                        Console.WriteLine($"{user.Name} has connected.");
                        Broadcast($"{user.Name} has connected.");
                    }
                    catch (System.IO.IOException e) when (e.InnerException is SocketException)
                    {
                        Console.WriteLine("Connection attempt aborted: client disconnected.");
                        return;
                    }

                    try
                    {
                        user.Start();
                    }
                    catch (System.IO.IOException e) when (e.InnerException is SocketException)
                    {
                        Console.WriteLine("Connection aborted by client.");
                    }
                    finally
                    {
                        EndConnection(user);
                        Broadcast($"{user.Name} has disconnected.");
                        Console.WriteLine($"{user.Name} has disconnected.");
                    }
                }
            });
        }

        // when someone leaves, endconnection removes the connection from the list
        public void EndConnection(User user)
        {
            lock (Users)
            {
                Users.Remove(user);
            }
        }

        // Broadcast handles sending a message over the entire server (all players can see it regardless of their room)
        public void Broadcast(string msg)
        {
            lock (Users)
            {
                foreach (User user in Users)
                {
                    user.Connection.SendMessage(msg);
                }
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
                    if (user.CurrRoomId == (sender as User).CurrRoomId)
                    {
                        user.Connection.SendMessage(msg);
                    }
                }
            }
        }

        #region Handlers for events / If/Else Hell(TM)
        void HandleHelpEvent(object sender, EventArgs e)
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
'examine <object>' : Examine an item.
'examine self' : Look at yourself.
'go <direction>' : Move between rooms through valid doors.
'no' or 'n' : Decline.
'yes' or 'y' : Agree.
'say <message>' : Talk to the players in your room.");
        }

        #region Utility funcs for event handlers 
        // Utility func for getting the user's current room ID
        int GetCurrentRoomId(User sender)
        {
            return WorldMap.Rooms.FindIndex(a => a.Id == sender.CurrRoomId);
        }
        // Utility func for HandleWhoEvent, returns whether or not a room has NPC entities.
        bool hasEntities(User sender)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender)].Entities.Count != 0;
        }
        // Utility func for finding if a room has Things
        bool hasThings(User sender)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender)].Things.Count != 0;
        }
        // Utility func for finding if a room has Doors
        bool hasDoors(User sender)
        {
            return WorldMap.Rooms[GetCurrentRoomId(sender)].Doors.Count != 0;
        }
        // Utility function for string matching
        bool fuzzyEquals(string a, string b)
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        // Look gets the description of a room.
        void HandleLookEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            int index = GetCurrentRoomId(s);
            s.Connection.SendMessage($"You look around:\n{WorldMap.Rooms[index].Description}");
            if (hasThings(s))
            {
                s.Connection.SendMessage($"You see some interesting things: {string.Join(", ", WorldMap.Rooms[index].Things.Select(t => t.Actual))}.");
            }
            if (hasDoors(s))
            {
                s.Connection.SendMessage($"You see doors to: {string.Join(", ", WorldMap.Rooms[index].Doors.Select(t => t.Actual))}.");
            }
        }

        // Who gets a list of players currently in the sender's room.
        void HandleWhoEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            List<User> currentUsers = new List<User>();
            lock (Users)
            {
                foreach (User user in Users)
                {
                    if (user.CurrRoomId == s.CurrRoomId)
                    {
                        currentUsers.Add(user);
                    }
                }
                currentUsers.Remove(s);
            }
            if (hasEntities(s))
            {
                List<Entity> friendlies = new List<Entity>();
                List<Entity> meanies = new List<Entity>();
                List<Entity> sneakies = new List<Entity>();
                List<Entity> detectedSneakies = new List<Entity>();
                foreach (var i in WorldMap.Rooms[GetCurrentRoomId(s)].Entities)
                {
                    // each i is a reference to an entity
                    if (i.Actual.IsFriendly)
                    {
                        friendlies.Add(i.Actual);
                    }
                    else
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
                }
                if (friendlies.Count > 0)
                {
                    s.Connection.SendMessage($"You can see some friendlies: {string.Join(", ", friendlies.Select(t => t.Name))}");
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
                        s.Connection.SendMessage($"Although they are trying to be stealthy, you can see some other enemies lurking in the shadows: {string.Join(", ", detectedSneakies.Select(t => t.Name))}");
                        return;
                    }
                    s.Connection.SendMessage("You don't see anyone new, but you sense a strange presence.");
                    return;
                }
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
        }

        // Inventory gets the user's inventory.
        void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            User s = sender as User;
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
        void HandleNoEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You firmly shake your head no.");
        }

        // Yes says yes.
        void HandleYesEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You nod affirmatively.");
        }

        // Switch statement for handling events with params
        void HandleParameterizedEvent(object sender, ParameterizedEvent e)
        {
            User s = sender as User;
            string action = e.Action;
            switch (e.Command)
            {
                case "say":
                    RoomSay($"{User.FormatMessage(action, s.Name)}", s);
                    break;
                case "take":
                    HandleTake(s, action);
                    break;
                case "drop":
                    HandleDrop(s, action);
                    break;
                case "examine":
                    HandleExamine(s, action);
                    break;
                case "go":
                    HandleGo(s, action);
                    break;
                case "equip":
                    HandleEquip(s, action);
                    break;
                case "remove":
                    HandleRemove(s, action);
                    break;
                case "hold":
                    HandleHold(s, action);
                    break;
                default:
                    s.Connection.SendMessage("You cannot do that.");
                    break;
            }
        }

        /* HandleTake and HandleDrop are relatively similar functions that just perform the inverses of each other
         * HandleTake checks if the item a user wants to take exists in the context of the room they are in.
         * if item exists, remove it from the room's list of references and add it to the player's inventory list instead       
         */
        void HandleTake(User sender, string e)
        {
            int roomId = GetCurrentRoomId(sender);
            var target = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => fuzzyEquals(t.Actual.Name, e));
            
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
            lock (WorldMap.Rooms[roomId].Things)
            {
                if (WorldMap.Rooms[roomId].Things.Remove(target))
                {
                    sender.Inventory.AddToInventory(target.Actual);
                }
                else
                {
                    sender.Connection.SendMessage("That item is no longer there.");
                }
            }
        }

        /* Like Handletake, HandleDrop checks for an item's existence, but in this case it has to be in the 
         * requesting user's inventory.
         * if item exists, remove it from the user inventory's list of refs and add to the room's Things instead
         */
        void HandleDrop(User sender, string e)
        {
            int roomId = GetCurrentRoomId(sender);
            Thing target = sender.Inventory.Things.FirstOrDefault(t => fuzzyEquals(t.Name, e));

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

             XMLReference<Thing> thing = new XMLReference<Thing> { Actual = target };
             sender.Player.Drop(target);
             sender.Inventory.RemoveFromInventory(target);
             WorldMap.Rooms[roomId].Things.Add(thing);
        }

        void HandleHoldDrop(User sender, string e)
        {
            int roomID = GetCurrentRoomId(sender);
            var target = sender.Player.Held.FirstOrDefault(t => fuzzyEquals(t.Name, e));

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
            sender.Inventory.AddToInventory(target);
        }

        /* HandleExamine allows a user to examine an object or themselves
         * 'examine self' returns a description of the user's player entity, including what they're wearing and appearance.
         * 'examine <item>' returns the description of the item. The item can be either in the room or in the user's inventory.
         * If the item is in neither, there is no description.       
         */
        void HandleExamine(User sender, string e)
        {
            if (e == null)
            {
                sender.Connection.SendMessage("Examine what?");
                return;
            }

            if (fuzzyEquals(e, "self"))
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
                return;
            }

            int roomId = GetCurrentRoomId(sender);
            Thing thing = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => fuzzyEquals(t.Actual.Name, e))?.Actual ??
            sender.Inventory.Things.FirstOrDefault(t => fuzzyEquals(t.Name, e));
            if(thing == null)
            {
                sender.Connection.SendMessage("That does not exist here.");
                return;
            }
            if(!thing.Commands.Contains("examine"))
            {
                sender.Connection.SendMessage("You cannot examine that.");
                return;
            }

            sender.Connection.SendMessage($"You examine the {e}.");
            sender.Connection.SendMessage($"{thing.Description}");
        }

        void HandleEquip(User sender, string e)
        {
            var target = sender.Inventory.Things.FirstOrDefault(t => fuzzyEquals(t.Name, e));
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

                foreach (Thing thing in sender.Player.Equipped)
                {
                    if (target.Slot == thing.Slot)
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
                    return;
                }
            }
        }       

        void HandleRemove(User sender, string e)
        {
            var target = sender.Player.Equipped.FirstOrDefault(t => fuzzyEquals(t.Name, e));
            if (target == null)
            {
                sender.Connection.SendMessage("You are not wearing that.");
                return;
            }
            
            if(target.Commands.Contains("remove"))
            {
                sender.Player.Unequip(target);
                sender.Inventory.AddToInventory(target);
                return;
            }
           
            sender.Connection.SendMessage("You cannot remove that.");
        }

       void HandleHold(User sender, string e)
       {
            var target = sender.Inventory.Things.FirstOrDefault(t => fuzzyEquals(t.Name, e));

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
            int currentRoomId = GetCurrentRoomId(sender);
            int numDoors = WorldMap.Rooms[currentRoomId].Doors.Select(t => t.Actual).Count();
            var directionGone = WorldMap.Rooms[currentRoomId].Doors.FirstOrDefault(t => fuzzyEquals(t.Actual.Direction, e));
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
                sender.CurrRoomId = directionGone.Actual.RoomsIConnect[1];
            }
            
            if(!directionGone.Actual.Locked)
            {
                proceed();
                return;
            }
              
            if(sender.Player.Stats.Dexterity >= directionGone.Actual.minDexterity)
            {
                sender.Connection.SendMessage("Although the door is locked, you are deft enough to pick your way in.");
                proceed();
                return;
            }
                
            if(directionGone.Actual.HasKey)
            {
                sender.Connection.SendMessage("This door is locked and has a key. Look around!");
            }
            else
            {
                sender.Connection.SendMessage("You are unable to open the door.");
            }
        }
       #endregion
    }
}
