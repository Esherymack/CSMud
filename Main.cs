/* CSMud - Rev 1
 * Author: Madison Tibbett
 * Last Modified: 02/22/2019
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace CSMud
{
    /* The Server class is a simple TCP server to accept connections
     * Server also holds the main function.
     * this is very largely based off of this forum post:
     * https://bytes.com/topic/c-sharp/answers/275416-c-mud-telnet-server
     * (okay, like 50% - i just mostly refactored it)
     * and also from Microsoft's notes:
     * https://docs.microsoft.com/en-us/dotnet/csharp/
     * 
     * My major modifications are as follows:
     * - Moved the locks into the methods that actually affect connections, cleaner code 
     *      - TODO: Possibly refactor for async/await instead of threading?
     * - Lots of QOL refactoring
     * - Added 'user' name for differentiating between users
     * - Changed ProcessLine to handle all incoming messages, differentiate between messages and commands
     * - Designated SendMessage for sending chat messages.
     * - Added connection and disconnection messages on the server for other users
     * - Added ambient periodic message sending (HeartBeat)
     * - Broadcast function
     */
    public class Server
    {
        // define static port number and backlog size
        const int port = 8088;
        const int backLog = 20;

        // a server has a world
        public World World
        { get; private set; }

        // a ListenSocket is a socket
        private Socket ListenSocket
        { get; }

        // Constructor
        public Server()
        {
            // Create a new world on the server
            this.World = new World();
            /* 
            * Instantiate Socket object 'server'
            * 'AddressFamily.InterNetwork' refers to the AddressFamily enum - specifically addresses for IPv4
            * 'SocketType.Stream' refers to the SocketType enum - Stream supports two-way conn-based byte streams without duplication of data
            * 'ProtocolType.Tcp' refers to the ProtocolType enum - Tcp is a TCP server
            */
            this.ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /*
            * Socket.Bind method takes a local endpoint
            * IPEndPoint class represents a network endpoint as an IP address and port number
            */
            this.ListenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        // Start starts the server. 
        public void Start()
        {
            /*
            * Socket.Listen method takes an integer and places the socket into a listening state. 
            * backLog is the maximum length of pending connections queue
            */
            ListenSocket.Listen(backLog);
            while(true)
            {
                // Accept incoming connections and create new Connections 
                Socket conn = ListenSocket.Accept();
                World.NewConnection(new Connection(conn, this.World));
            }
        }

        // main 
        static void Main(string[] args)
        {
            new Server().Start();
        }
    }

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
            lock(Connections)
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
                    using (StreamWriter writer = conn.Writer)
                    {
                        writer.WriteLine(msg);
                    }
                }
            }
        }
    }

    /* Connection is basically the "client" - you don't have to have it separate
     * Just telnet the connection
     * 'telnet 192.168.1.111 8088' for example
     */
    public class Connection
    {
        /*
         * We need a new Socket, as well as a StreamReader and StreamWriter 
         * StreamReader class reads characters from a byte stream in a particular encoding
         * StreamWriter writes characters to a stream in a particular encoding
         */
        Socket socket;

        // a LoopThread is a thread
        private System.Threading.Thread LoopThread
        { get; set; }

        // User property for the user's screen name
        public string User
        { get; set; }

        // a connection has a world
        public World World
        { get; }

        // Reader and Writer get expression-valued properties
        public StreamWriter Writer => new StreamWriter(new NetworkStream(socket, false));
        public StreamReader Reader => new StreamReader(new NetworkStream(socket, false));

        // Constructor
        public Connection(Socket socket, World world)
        {
            this.socket = socket;
            this.World = world;
            /* Every connection gets a reader and writer
            * the writer is set to auto-flush for every user - this helps get messages displayed properly to each individual user
            * Get the user's screen name for later use
            */
            GetLogin();
            this.LoopThread = new System.Threading.Thread(ClientLoop);
        }

        public void Start()
        {
            // Start a new Thread running ClientLoop()
            LoopThread.Start();
        }

        //  ClientLoop basically handles the sending and receiving of messages as well as handling incoming and outgoing connections.
        void ClientLoop()
        {
            try
            {
                // Welcome the user to the game
                OnConnect();
                while (true)
                {
                    // Get a message that's sent to the server
                    using (StreamReader reader = this.Reader)
                    {
                       string line = reader.ReadLine();
                       // if the line is empty, or if the line says "quit," then break the loop
                            // TODO : Move the "quit" condition to a command rather than something that happens in here
                       if (line == null || line == "quit")
                       {
                           break;
                       }
                       // otherwise, we process the line
                       else
                       {
                           ProcessLine(line);
                       }
                    }    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
            finally
            {
                // when a user disconnects, tell the server they've left
                this.World.Broadcast($"{this.User} has disconnected.");
                Console.WriteLine($"{this.User} has disconnected.");
                OnDisconnect();
            }
        }

        // GetLogin gets a screen name for individual users. This name is displayed for messages sent, actions, and connection/disconnection
        void GetLogin()
        {
            using (StreamWriter writer = this.Writer)
            {
                writer.Write("Please enter a name: ");
            }
            using (StreamReader reader = this.Reader)
            {
                string user = reader.ReadLine();
                // if the user does not enter a line in for their "name," just call them "Someone"
                if (user == "")
                {
                    user = "Someone";
                }
                this.User = user;
            }
        }

        // OnConnect handles the welcome messages and tells the server client that someone has connected.
        void OnConnect()
        {
            using (StreamWriter writer = this.Writer)
            {
                writer.WriteLine("Welcome!");
                writer.WriteLine("Send 'quit' to exit.");
                writer.WriteLine("Send 'help' for help.");
            }
        }

        // OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
        void OnDisconnect()
        {
            this.World.EndConnection(this);
            socket.Close();
            this.LoopThread.Abort();
        }

        /*
         * ProcessLine handles organizing messages on the MUD server
         * as of right now it just trims and sends the message to SendMessasge
         */
        void ProcessLine(string line)
        {
            line = line.Trim();
            SendMessage(line);
        }

        // SendMessage handles sending speech messages on the MUD server
        void SendMessage(string line)
        {
            this.World.Broadcast($"{this.User} says, '{line}'");
        }
    }

    // The Commands class holds all user commands other than movement
    public class Commands
    {
        
    }

    // The Movement class is movement-specific commands
    public class Movement
    {
        public enum Directions
        {
            Undefined, North, South, East, West, Up, Down, NorthEast, NorthWest, SouthEast, SouthWest, In, Out
        };
    }

    // The Map class is all map-generating specifics
    public class Map
    {
    }

    // The Objects class holds all interactable object interactions
    public class Objects
    {
    }

    // The Entity class holds NPC and enemy specifics
    public class Entity
    {
    }

    // The Timer class simply starts and stops a timer for the run.
    public class MapTimer
    {
    }

    // The Combat class holds commands specific to combat. 
    public class Combat
    {
    }
}