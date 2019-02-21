/* CSMud - Rev 1
 * Author: Madison Tibbett
 * Last Modified: 02/20/2019
 */

using System;
using System.Collections;
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
     */
    public class Server
    {
        // define static port number and backlog size
        const int port = 8088;
        const int backLog = 20;

        // main 
        static void Main(string[] args)
        {
            /* 
             * Instantiate Socket object 'server'
             * 'AddressFamily.InterNetwork' refers to the AddressFamily enum - specifically addresses for IPv4
             * 'SocketType.Stream' refers to the SocketType enum - Stream supports two-way conn-based byte streams without duplication of data
             * 'ProtocolType.Tcp' refers to the ProtocolType enum - Tcp is a TCP server
             */            
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            /*
             * Socket.Bind method takes a local endpoint
             * IPEndPoint class represents a network endpoint as an IP address and port number
             */
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            /*
             * Socket.Listen method takes an integer and places the socket into a listening state. 
             * backLog is the maximum length of pending connections queue
             */
            server.Listen(backLog);
            while(true)
            {
                /*
                 * Accept incoming connections and create new Connections 
                 */
                Socket conn = server.Accept();
                new Connection(conn);
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
        // User property for the user's screen name
        public string User { get; set; }

        /*
         * Last, we need an ArrayList to hold our connections
         * This could be avoided by using async/await code, but because this is using threading, we need some sort of data structure to hold the connections
         * I've seen people using hashtables and dictionaries but really i think an array makes sense.
         */
        static ArrayList connections = new ArrayList();

        /*
         * Constructor
         */
        public Connection(Socket socket)
        {
            this.socket = socket;
            // Every connection gets a reader and writer
            // the writer is set to auto-flush for every user - this helps get messages displayed properly to each individual user
            Reader = new StreamReader(new NetworkStream(socket, false));
            Writer = new StreamWriter(new NetworkStream(socket, true));
            Writer.AutoFlush = true;
            // Get the user's screen name for later use
            GetLogin();
            // Call Beat to start the timer when a new connection is made.
            // Individual users get their ambient message every 45 seconds, independent of other users
            Beat();
            /*
             * Start a new Thread running ClientLoop()
             */
            new System.Threading.Thread(ClientLoop).Start();
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
                while(true)
                {
                    // is this necessary? I set Writer.AutoFlush = true above.
                    foreach(Connection conn in connections)
                    {
                        conn.Writer.Flush();
                    }
                    // Get a message that's sent to the server
                    string line = Reader.ReadLine();
                    // if the line is empty, or if the line says "quit," then break the loop
                    if(line == null || line == "quit")
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
            catch(Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
            finally
            {
                // when a user disconnects, tell the server they've left
                string msg = $"{this.User} has disconnected.";
                Broadcast(msg);
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
            this.User = Reader.ReadLine();
        }

        /*
         * OnConnect handles the welcome messages and tells the server client that someone has connected.
         */
        void OnConnect()
        {
            Writer.WriteLine("Welcome!");
            Writer.WriteLine("Send 'quit' to exit.");
            Console.WriteLine($"{this.User} has connected.");
            lock(connections)
            {
                connections.Add(this);
            }
        }

        /*
         * OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
         */
        void OnDisconnect()
        {
            lock(connections)
            {
                connections.Remove(this);
            }
            Console.WriteLine($"{this.User} has disconnected.");
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
            Broadcast(msg);
        }

        /*
         * Broadcast handles sending a message over the server - y'know, broadcasting it.
         */
        void Broadcast(string msg)
        {
           lock(connections)
           {
               foreach(Connection conn in connections)
               {
                   conn.Writer.WriteLine(msg);
               }
           }
        }

        /*
         * Beat handles sending a periodic message over the server to serve as "ambiance."
         * Currently, the timer is set for 45 seconds.
         */
        void Beat()
        {
            // 45 seconds, a la milliseconds.
            Timer tmr = new Timer(45000);
            tmr.Elapsed += OnTimedEvent;
            tmr.AutoReset = true;
            tmr.Enabled = true;
        }
        /*
         * OnTimedEvent goes with Beat() and is the function containing whatever happens every time the timer runs out.
         */
        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Writer.WriteLine("The world is dark and silent.");
        }
    }

    // The Commands class holds all user commands other than movement
    public class Commands
    {
    }

    // The Movement class is movement-specific commands
    public class Movement
    {
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