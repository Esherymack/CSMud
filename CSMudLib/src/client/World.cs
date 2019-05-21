using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using CSMud.Events;
using CSMud.Thingamajig;
using CSMud.Utils;

namespace CSMud.Client
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

        public ProcessUnparameterizedCommand PUC { get; set; }

        public ProcessParameterizedCommand PPC { get; set; }

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

            // Generate the map
            WorldMap = new MapBuild();

            // Initialize command processors 
            PUC = new ProcessUnparameterizedCommand(WorldMap, Users);
            PPC = new ProcessParameterizedCommand(WorldMap, Users);

            foreach(Entity e in WorldMap.Entities)
            {
                e.PopulateInventory();
            }
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
                    user.Command.RaiseHelpEvent += PUC.HandleHelpEvent;
                    user.Command.RaiseLookEvent += PUC.HandleLookEvent;
                    user.Command.RaiseWhoEvent += PUC.HandleWhoEvent;
                    user.Command.RaiseInventoryQueryEvent += PUC.HandleInventoryQueryEvent;
                    user.Command.RaiseNoEvent += PUC.HandleNoEvent;
                    user.Command.RaiseYesEvent += PUC.HandleYesEvent;
                    user.Command.RaiseParameterizedEvent += PPC.HandleParameterizedEvent;
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
    }
}
