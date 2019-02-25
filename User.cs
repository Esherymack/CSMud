using System;

namespace CSMud
{
    public class User : IDisposable
    {
        public Connection Connection { get; }

        // a connection has a world
        private World World
        { get; }

        public string Name { get; }

        public User(Connection conn, World world, string name)
        {
            this.Connection = conn;
            this.World = world;
            this.Name = name;
        }

        // OnConnect handles the welcome messages and tells the server client that someone has connected.
        private void OnConnect()
        {
            // multiline string literal is making me very upset, many conniptions
            Connection.SendMessage("Welcome!\nSend 'quit' to exit.\nSend 'help' for help.");
        }

        // OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
        private void OnDisconnect()
        {
            this.World.EndConnection(this);
            this.World.Broadcast($"{this.Name} has disconnected.");
            Console.WriteLine($"{this.Name} has disconnected.");
        }

        public void Start()
        {
            // Welcome the user to the game
            OnConnect();
            while (true)
            {
                // Get a message that's sent to the server
                string line = Connection.ReadMessage();
                // if the line is empty, or if the line says "quit," then break the loop
                // TODO : Move the "quit" condition to a command rather than something that's checked here
                if (line == null || line == "quit")
                {
                    break;
                }
                // otherwise, we process the line
                else
                {
                    string message = FormatMessage(line);
                    this.World.Broadcast(message);
                }
            }
        }

        /*
		 * ProcessLine handles organizing messages on the MUD server
		 * as of right now it just trims and sends the message to SendMessasge
		 */
        string FormatMessage(string line)
        {
            return $"{this.Name} says, '{line.Trim()}'";
        }

        public void Dispose()
        {
            ((IDisposable)Connection).Dispose();
        }
    }
}
