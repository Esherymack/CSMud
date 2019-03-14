using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace CSMud
{
    // A World holds connections and handles broadcasting messages from those connections
    public class World
    {
        // a world has connections
        public List<User> Users
        { get; }

        // a beat is a periodic message sent over the server
        private Timer Beat
        { get; }

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
            Broadcast("The world is dark and silent.");
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

        // Broadcast handles sending a message over the server - y'know, broadcasting it.
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


        #region Handlers for events 
        void HandleHelpEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage(@"Help:
'help' : Display this message.
'quit' : Exit the game.
'look' : Look at the room you are in.
'inventory' or 'i' : Display inventory.
'take <object>' : Take an item.
'drop <object>' : Drop an item in your inventory.
'examoine <object>' : Examine an item.
'no' or 'n' : Decline.
'yes' or 'y' : Agree.
'say <message>' : Broadcast a message");
        }

        int getCurrentRoomId(object sender)
        {
            return WorldMap.Rooms.FindIndex(a => a.Id == (sender as User).CurrRoomId);
        }

        void HandleLookEvent(object sender, EventArgs e)
        {
            int index = getCurrentRoomId(sender);
            (sender as User).Connection.SendMessage($"You look around:\n{WorldMap.Rooms[index].Description}");
            (sender as User).Connection.SendMessage($"{string.Join(", ", WorldMap.Rooms[index].Things.Select(t => t.Actual))}");
        }

        void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            if ((sender as User).Inventory.Empty)
            {
                (sender as User).Connection.SendMessage($"You turn out your pockets. You have nothing.");
            }
            else
            {
                (sender as User).Connection.SendMessage($"Your inventory consists of:\n {(sender as User).Inventory.ToString()}");
            }
        }

        void HandleNoEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You firmly shake your head no.");
        }

        void HandleYesEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You nod affirmatively.");
        }

        void HandleParameterizedEvent(object sender, ParameterizedEvent e)
        {
            switch (e.Command)
            {
                case "say":
                    Broadcast($"{User.FormatMessage(e.Action, (sender as User).Name)}");
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
                default:
                    (sender as User).Connection.SendMessage("You cannot do that.");
                    break;
            }
        }

        void HandleTake(object sender, ParameterizedEvent e)
        {
            int roomId = getCurrentRoomId(sender);
            var target = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => string.Equals(t.Actual.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                (sender as User).Connection.SendMessage("No such object exists.");
            }
            else
            {
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

        void HandleDrop(object sender, ParameterizedEvent e)
        {
            int roomId = getCurrentRoomId(sender);
            var target = (sender as User).Inventory.Things.FirstOrDefault(t => string.Equals(t.Name, e.Action.Trim(), StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                (sender as User).Connection.SendMessage("No such object exists.");
            }
            else
            {
                XMLReference<Thing> thing = new XMLReference<Thing>();
                thing.Actual = target;
                (sender as User).Inventory.RemoveFromInventory(target);
                WorldMap.Rooms[roomId].Things.Add(thing);
            }
        }

        void HandleExamine(object sender, ParameterizedEvent e)
        {

        }

        #endregion
    }
}
