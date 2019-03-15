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
            this.Users = new List<User>();
            // Create a beat on the server
            this.Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            this.Beat.Elapsed += OnTimedEvent;

            // Generate the map last
            this.WorldMap = new MapBuild();
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        void OnTimedEvent(Object source, ElapsedEventArgs e)
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
                    else if (username == "")
                    {
                        username = "Someone";
                    }
                    User user = new User(conn, this, username);
                    lock (Users)
                    {
                        Users.Add(user);
                    }

                    #region Subscribe the user to events. 
                    user.RaiseHelpEvent += this.HandleHelpEvent;
                    user.RaiseLookEvent += this.HandleLookEvent;
                    user.RaiseWhoEvent += this.HandleWhoEvent;
                    user.RaiseInventoryQueryEvent += this.HandleInventoryQueryEvent;
                    user.RaiseNoEvent += this.HandleNoEvent;
                    user.RaiseYesEvent += this.HandleYesEvent;
                    user.RaiseParameterizedEvent += this.HandleParameterizedEvent;
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
                        this.EndConnection(user);
                        this.Broadcast($"{user.Name} has disconnected.");
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
        public void RoomSay(string msg, object sender)
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

        // Utility func for getting the user's current room ID
        int getCurrentRoomId(object sender)
        {
            return WorldMap.Rooms.FindIndex(a => a.Id == (sender as User).CurrRoomId);
        }

        // Look gets the description of a room.
        void HandleLookEvent(object sender, EventArgs e)
        {
            int index = getCurrentRoomId(sender);
            (sender as User).Connection.SendMessage($"You look around:\n{WorldMap.Rooms[index].Description}");
            if (hasThings(sender))
            {
                (sender as User).Connection.SendMessage($"You see some interesting things: {string.Join(", ", WorldMap.Rooms[index].Things.Select(t => t.Actual))}.");
            }
            if (hasDoors(sender))
            {
                (sender as User).Connection.SendMessage($"You see doors to: {string.Join(", ", WorldMap.Rooms[index].Doors.Select(t => t.Actual))}.");
            }
        }

        // Who gets a list of players currently in the sender's room.
        void HandleWhoEvent(object sender, EventArgs e)
        {
            List<User> currentUsers = new List<User>();
            lock (Users)
            {
                foreach (User user in Users)
                {
                    if (user.CurrRoomId == (sender as User).CurrRoomId)
                    {
                        currentUsers.Add(user);
                    }
                }
                currentUsers.Remove((sender as User));
            }
            if (hasEntities(sender))
            {
                List<Entity> friendlies = new List<Entity>();
                List<Entity> meanies = new List<Entity>();
                List<Entity> sneakies = new List<Entity>();
                foreach (var i in WorldMap.Rooms[getCurrentRoomId(sender)].Entities)
                {
                    // (sender as User).Connection.SendMessage($"{i}");
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
                    (sender as User).Connection.SendMessage($"You can see some friendlies: {string.Join(", ", friendlies.Select(t => t.Name))}");
                }
                if (meanies.Count > 0)
                {
                    (sender as User).Connection.SendMessage($"You can see some enemies: {string.Join(", ", meanies.Select(t => t.Name))}");
                }
                if (sneakies.Count > 0)
                {
                    // TODO: If user can percieve above certain value, they can see and interact with hidden enemies (after implementing Stats.cs)
                    (sender as User).Connection.SendMessage("You don't see anyone new, but you sense a strange presence.");
                }
            }
            // if there is one player
            if (currentUsers.Count == 1)
            {
                (sender as User).Connection.SendMessage($"You see your ally, {string.Join(", ", currentUsers.Select(t => t.Name))}.");
            }
            // if there are multiple players
            else if (currentUsers.Count > 1)
            {
                (sender as User).Connection.SendMessage($"You see your allies, {string.Join(", ", currentUsers.Select(t => t.Name))}.");
            }
            // if there are no other players
            else
            {
                (sender as User).Connection.SendMessage("You have no allies nearby.");
            }
        }

        // Utility func for HandleWhoEvent, returns whether or not a room has NPC entities.
        bool hasEntities(object sender)
        {
            if (WorldMap.Rooms[getCurrentRoomId(sender)].Entities.Count != 0)
            {
                return true;
            }
            return false;
        }
        // Utility func for finding if a room has Things
        bool hasThings(object sender)
        {
            if (WorldMap.Rooms[getCurrentRoomId(sender)].Things.Count != 0)
            {
                return true;
            }
            return false;
        }
        bool hasDoors(object sender)
        {
            if (WorldMap.Rooms[getCurrentRoomId(sender)].Doors.Count != 0)
            {
                return true;
            }
            return false;
        }

        // Inventory gets the user's inventory.
        void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            if ((sender as User).Inventory.Empty)
            {
                (sender as User).Connection.SendMessage($"You turn out your pockets. You have nothing.");
            }
            else
            {
                (sender as User).Connection.SendMessage($"Your inventory consists of:\n{(sender as User).Inventory.ToString()}");
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
            switch (e.Command)
            {
                case "say":
                    RoomSay($"{User.FormatMessage(e.Action, (sender as User).Name)}", sender);
                    break;
                case "take":
                    HandleTake(sender, e);
                    break;
                case "drop":
                    HandleDrop(sender, e);
                    break;
                case "examine":
                    HandleExamine(sender, e);
                    break;
                case "go":
                    HandleGo(sender, e);
                    break;
                case "equip":
                    HandleEquip(sender, e);
                    break;
                case "remove":
                    HandleRemove(sender, e);
                    break;
                case "hold":
                    HandleHold(sender, e);
                    break;
                default:
                    (sender as User).Connection.SendMessage("You cannot do that.");
                    break;
            }
        }

        /* HandleTake and HandleDrop are relatively similar functions that just perform the inverses of each other
         * HandleTake checks if the item a user wants to take exists in the context of the room they are in.
         * if item exists, remove it from the room's list of references and add it to the player's inventory list instead       
         */
        void HandleTake(object sender, ParameterizedEvent e)
        {
            int roomId = getCurrentRoomId(sender);
            var target = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => string.Equals(t.Actual.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                (sender as User).Connection.SendMessage("No such object exists.");
            }
            else if (!target.Actual.Commands.Contains("take"))
            {
                (sender as User).Connection.SendMessage("You cannot take that.");
            }
            else
            {
                // Prevent the chance that two users try to take the same item at the same time.
                lock (WorldMap.Rooms[roomId].Things)
                {
                    if (WorldMap.Rooms[roomId].Things.Remove(target))
                    {
                        (sender as User).Inventory.AddToInventory(target.Actual);
                    }
                    else
                    {
                        (sender as User).Connection.SendMessage("That item is no longer there.");
                    }
                }
            }
        }

        /* Like Handletake, HandleDrop checks for an item's existence, but in this case it has to be in the 
         * requesting user's inventory.
         * if item exists, remove it from the user inventory's list of refs and add to the room's Things instead
         */
        void HandleDrop(object sender, ParameterizedEvent e)
        {
            int roomId = getCurrentRoomId(sender);
            var target = (sender as User).Inventory.Things.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                HandleHoldDrop(sender, e);
            }
            else if (!target.Commands.Contains("drop"))
            {
                (sender as User).Connection.SendMessage("You cannot drop that.");
            }
            else
            {
                XMLReference<Thing> thing = new XMLReference<Thing> { Actual = target };
                (sender as User).Inventory.RemoveFromInventory(target);
                WorldMap.Rooms[roomId].Things.Add(thing);
            }
        }

        void HandleHoldDrop(object sender, ParameterizedEvent e)
        {
            int roomID = getCurrentRoomId(sender);
            var target = (sender as User).Player.Held.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                (sender as User).Connection.SendMessage("You are not holding that.");
            }
            else if (!target.Commands.Contains("drop"))
            {
                (sender as User).Connection.SendMessage("You cannot drop that.");
            }
            else
            {
                (sender as User).Player.Drop(target);
                (sender as User).Inventory.AddToInventory(target);
            }
        }

        /* HandleExamine allows a user to examine an object or themselves
         * 'examine self' returns a description of the user's player entity, including what they're wearing and appearance.
         * 'examine <item>' returns the description of the item. The item can be either in the room or in the user's inventory.
         * If the item is in neither, there is no description.       
         */
        void HandleExamine(object sender, ParameterizedEvent e)
        {
            int roomId = getCurrentRoomId(sender);
            var target = e.Action;
            if (target == null)
            {
                (sender as User).Connection.SendMessage("Examine what?");
            }
            else
            {
                if (string.Equals(target, "self", StringComparison.OrdinalIgnoreCase))
                {
                    (sender as User).Connection.SendMessage("You look yourself over.");
                    if ((sender as User).Player.Equipped.Count > 0)
                    {
                        (sender as User).Connection.SendMessage($"Equipped: {string.Join(", ", (sender as User).Player.Equipped)}");
                    }
                    if ((sender as User).Player.Held.Count > 0)
                    {
                        (sender as User).Connection.SendMessage($"Held: {string.Join(", ", (sender as User).Player.Held)}");
                    }
                }
                else
                {
                    XMLReference<Thing> thing = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => string.Equals(t.Actual.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (thing != null)
                    {
                        if (thing.Actual.Commands.Contains("examine"))
                        {
                            (sender as User).Connection.SendMessage($"You examine the {target}.");
                            (sender as User).Connection.SendMessage($"{thing.Actual.Description}");
                        }
                        else
                        {
                            (sender as User).Connection.SendMessage("You cannot examine that.");
                        }
                    }
                    else
                    {
                        Thing theThing = (sender as User).Inventory.Things.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (theThing != null)
                        {
                            if (thing.Actual.Commands.Contains("examine"))
                            {
                                (sender as User).Connection.SendMessage($"You examine the {target}.");
                                (sender as User).Connection.SendMessage($"{theThing.Description}");
                            }
                            else
                            {
                                (sender as User).Connection.SendMessage("You cannot examine that.");
                            }
                        }
                    }
                }
            }
        }

            void HandleEquip(object sender, ParameterizedEvent e)
            {
                var target = (sender as User).Inventory.Things.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    (sender as User).Connection.SendMessage("You must have the object in your inventory.");
                }
                else if (target.Commands.Contains("equip"))
                {
                    (sender as User).Inventory.RemoveFromInventory(target);
                    (sender as User).Player.Equip(target);
                }
                else
                {
                    (sender as User).Connection.SendMessage("You cannot wear that.");
                }
            }

            void HandleRemove(object sender, ParameterizedEvent e)
            {
                var target = (sender as User).Player.Equipped.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    (sender as User).Connection.SendMessage("You are not wearing that.");
                }
                else if(target.Commands.Contains("remove"))
                {
                    (sender as User).Player.Unequip(target);
                    (sender as User).Inventory.AddToInventory(target);
                }
                else
                {
                    (sender as User).Connection.SendMessage("You cannot remove that.");
                }
            }

            void HandleHold(object sender, ParameterizedEvent e)
            {
                var target = (sender as User).Inventory.Things.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
                if (target == null)
                {
                    (sender as User).Connection.SendMessage("You must have the object in your inventory.");
                }
                else if (target.Commands.Contains("hold"))
                {
                    (sender as User).Inventory.RemoveFromInventory(target);
                    (sender as User).Player.Hold(target);
                }
                else
                {
                    (sender as User).Connection.SendMessage("You cannot hold that.");
                }
            }

            /* HandleGo handles moving between rooms.
             * TODO: Find a better way to handle the RoomsIConnect list - currently, in the XML, rooms must be 
             * placed in the order of CURRENT ROOM in index 0 and CONNECTING ROOM in index 1    
             * there's probably a better way to handle this situation.
             */
            void HandleGo(object sender, ParameterizedEvent e)
            {
                int currentRoomId = getCurrentRoomId(sender);
                int numDoors = WorldMap.Rooms[currentRoomId].Doors.Select(t => t.Actual).Count();
                if (numDoors == 0)
                {
                    (sender as User).Connection.SendMessage("There are no doors here. You cannot go anywhere.");
                }
                else
                {
                    var directionGoing = e.Action;
                    var directionGone = WorldMap.Rooms[currentRoomId].Doors.FirstOrDefault(t => string.Equals(t.Actual.Direction, directionGoing.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (directionGone != null)
                    {
                        (sender as User).CurrRoomId = directionGone.Actual.RoomsIConnect[1];
                    }
                    else
                    {
                        (sender as User).Connection.SendMessage("There is no door there.");
                    }
                }
            }

            #endregion
        }
    }
