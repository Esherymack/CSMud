﻿/* CSMud - Rev 1
 * Author: Madison Tibbett
 * Last Modified: 02/22/2019
 */

using System;
using System.Collections;
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
     * (okay, like 80%)
     * and also from Microsoft's notes:
     * https://docs.microsoft.com/en-us/dotnet/csharp/
     * 
     * My major modifications are as follows:
     * - Moved the locks down into the methods that actually affect connections, cleaner code 
     *      - TODO: Possibly refactor for async/await instead of threading?
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

        private Socket ListenSocket
        { get; }

        public Server()
        {
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

        public void Start()
        {
            /*
            * Socket.Listen method takes an integer and places the socket into a listening state. 
            * backLog is the maximum length of pending connections queue
            */
            ListenSocket.Listen(backLog);
            while(true)
            {
                /*
                * Accept incoming connections and create new Connections 
                */
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

    public class World
    {
        // a world has connections
        public List<Connection> Connections
        { get; }

        private Timer Beat
        { get; }

        public World()
        {
            this.Connections = new List<Connection>();
            /*
            * Beat handles sending a periodic message over the server to serve as "ambiance."
            * Currently, the timer is set for 45 seconds.
            */
            this.Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            this.Beat.Elapsed += OnTimedEvent;
        }

        /*
        * OnTimedEvent goes with Beat() and is the function containing whatever happens every time the timer runs out.
        */
        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            string msg = "The world is dark and silent.";
            Broadcast(msg);
        }

        // add new Connections to the world
        public void NewConnection(Connection conn)
        {
            string msg = $"{conn.User} has connected.";
            Console.WriteLine(msg);
            Broadcast(msg);
            lock (Connections)
            {
                Connections.Add(conn);
            }
            conn.Start();
        }

        public void EndConnection(Connection conn)
        {
            lock(Connections)
            {
                Connections.Remove(conn);
            }
        }

        /*
        * Broadcast handles sending a message over the server - y'know, broadcasting it.
        */
        public void Broadcast(string msg)
        {
            lock (Connections)
            {
                foreach (Connection conn in Connections)
                {
                    conn.Writer.WriteLine(msg);
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
        public StreamReader Reader;
        public StreamWriter Writer;

        private System.Threading.Thread LoopThread
        { get; set; }

        // User property for the user's screen name
        public string User
        { get; set; }

        // a connection has a world
        public World World
        { get; }

        /*
         * Constructor
         */
        public Connection(Socket socket, World world)
        {
            this.socket = socket;
            this.World = world;
            // Every connection gets a reader and writer
            // the writer is set to auto-flush for every user - this helps get messages displayed properly to each individual user
            Reader = new StreamReader(new NetworkStream(socket, false));
            Writer = new StreamWriter(new NetworkStream(socket, true));
            Writer.AutoFlush = true;
            // Get the user's screen name for later use
            GetLogin();
            this.LoopThread = new System.Threading.Thread(ClientLoop);
        }

        public void Start()
        {
            /*
            * Start a new Thread running ClientLoop()
            */
            LoopThread.Start();
        }

        /*
         * ClientLoop basically handles the sending and receiving of messages as well as handling incoming and outgoing connections.
         */
        void ClientLoop()
        {
            try
            {
                // Welcome the user to the game
                OnConnect();
                while (true)
                {
                    // Get a message that's sent to the server
                    string line = Reader.ReadLine();
                    // if the line is empty, or if the line says "quit," then break the loop
                    if (line == null || line == "quit")
                    {
                        break;
                    }
                    else if (line == "help")
                    {

                    }
                    // otherwise, we process the line
                    else
                    {
                        ProcessLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
            finally
            {
                socket.Close();
                OnDisconnect();
            }
        }

        /*
         * GetLogin gets a screen name for individual users. This name is displayed for messages sent, actions, and connection/disconnection
         */
        void GetLogin()
        {
            Writer.Write("Please enter a name: ");
            string user = Reader.ReadLine();
            if (user == "")
            {
                user = "Someone";
            }
            this.User = user;
        }

        /*
         * OnConnect handles the welcome messages and tells the server client that someone has connected.
         */
        void OnConnect()
        {
            Writer.WriteLine("Welcome!");
            Writer.WriteLine("Send 'quit' to exit.");
            Writer.WriteLine("Send 'help' for help.");
        }

        /*
         * OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
         */
        void OnDisconnect()
        {
            // when a user disconnects, tell the server they've left
            string msg = $"{this.User} has disconnected.";
            this.World.Broadcast(msg);
            Console.WriteLine(msg);
            this.LoopThread.Abort();
            this.World.EndConnection(this);
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

        /*
         * SendMessage handles sending speech messages on the MUD server
         */
        void SendMessage(string line)
        {
            string msg = $"{this.User} says, '{line}'";
            this.World.Broadcast(msg);
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