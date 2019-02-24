using System;
using System.Collections.Generic;
using System.Timers;

namespace CSMud
{
    // A World holds connections and handles broadcasting messages from those connections
    public class World
    {
        // a world has connections
        public List<Connection> Connections
        { get; }

        // a beat is a periodic message sent over the server
        private Timer Beat
        { get; }

        // constructor
        public World()
        {
            // create a list object of connections
            this.Connections = new List<Connection>();
            // Create a beat on the server
            this.Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            this.Beat.Elapsed += OnTimedEvent;
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Broadcast("The world is dark and silent.");
        }

        // add new Connections to the world
        public void NewConnection(Connection conn)
        {
            // tell the server that a user has connected
            Console.WriteLine($"{conn.User} has connected.");
            Broadcast($"{conn.User} has connected.");
            // add the new connection to the list
            lock (Connections)
            {
                Connections.Add(conn);
            }
            // start the connection
            conn.Start();
        }

        // when someone leaves, endconnection removes the connection from the list
        public void EndConnection(Connection conn)
        {
            lock (Connections)
            {
                Connections.Remove(conn);
            }
        }

        // Broadcast handles sending a message over the server - y'know, broadcasting it.
        public void Broadcast(string msg)
        {
            lock (Connections)
            {
                foreach (Connection conn in Connections)
                {
                    conn.SendMessage(msg);
                }
            }
        } 
    }
}
