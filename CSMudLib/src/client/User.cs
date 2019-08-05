using System;
using CSMud.Entity;
using CSMud.Events;
using CSMud.Utils;
using System.Timers;
using System.Linq;

namespace CSMud.Client
{
    public class User : IDisposable
    {
        public Connection Connection { get; }

        public HandleCommand Command { get; set; }

        // a user belongs to a world
        private World World { get; }

        public WorldTimer wt { get; set; }

        // a user has a unique inventory
        public Inventory Inventory { get; set; }

        // a user is set in a specific room
        public int CurrRoomId { get; set; }

        public Timer Beat { get; set; }

        // a user has a player object holding their "profile"
        // this consists of things they are wearing, things they are holding, and their stats
        public Player Player { get; set; }

        // a user has a name
        public string Name { get; }

        public User(Connection conn, World world, string name)
        {
            Connection = conn;
            Command = new HandleCommand(this);
            World = world;
            wt = new WorldTimer(this);
            Name = name;
            Player = new Player(name);
            Inventory = new Inventory();
            Inventory.setCarryCapacity(Player.Stats.Strength);
            CurrRoomId = 0001;

            // Create a beat on the server
            Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            Beat.Elapsed += wt.OnTimedEvent;
        }

        // OnConnect handles the welcome messages and tells the server client that someone has connected.
        private void OnConnect()
        {
            // multiline string literal is making me very upset, many conniptions
            // using \n new lines works on linux, not so much on windows
            Connection.SendMessage(@"Welcome!
Send 'quit' to exit.
Send 'help' for help.");
        }

        private void GlobalMessage()
        {
            Connection.SendMessage(@"This is a placeholder daily message");
        }

        // OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
        private void OnDisconnect()
        {
            World.EndConnection(this);
            World.Broadcast($"{Name} has disconnected.", World.Users);
            Console.WriteLine($"{Name} has disconnected.");
        }

        public void Start()
        {
            // Welcome the user to the game
            OnConnect();
            // Print global message
            GlobalMessage();

            while (true)
            {
                string line = Connection.ReadMessage();
                string[] splitLine = line.Split(new char[] { ' ' }, 2);

                if (splitLine == null || splitLine[0] == "quit")
                {
                    break;
                }
                // Get a message that's sent to the server
                Command.ProcessCommand(splitLine);
            } 
        }
        
        // Sends the room ambiance specific to the user's current room.
        public void Ping()
        {
            int idx = World.WorldMap.Rooms.FindIndex(t => t.Id == CurrRoomId);
            Connection.SendMessage(World.WorldMap.Rooms[idx].Ambient);
        }

        /*
		 * ProcessLine handles organizing messages on the MUD server
		 * as of right now it just trims and sends the message to SendMessasge
		 */
        public static string FormatMessage(string line, string name)
        {
            return $"{name} says, '{line.Trim()}'";
        }

        public void Dispose()
        {
            ((IDisposable)Connection).Dispose();
        }
    }
}
