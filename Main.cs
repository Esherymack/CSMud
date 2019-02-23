﻿/* CSMud - Rev 1
 * Author: Madison Tibbett
 * Last Modified: 02/22/2019
 */

using System.Net;
using System.Net.Sockets;

namespace CSMud
{
    /* The Server class is a simple TCP server to accept connections
     * Server also holds the main function.
     * this is inspired by this forum post:
     * https://bytes.com/topic/c-sharp/answers/275416-c-mud-telnet-server
     * and also from Microsoft's notes:
     * https://docs.microsoft.com/en-us/dotnet/csharp/
     * 
     * My major modifications are as follows:
     * - Moved the locks into the methods that actually affect connections, cleaner code 
     *      - TODO: Possibly refactor for async/await instead of threading?
     * - Lots of QOL refactoring - World now holds Connections instead of Server holding Connections, a Connection has a World
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

        // main just starts a new server 
        static void Main(string[] args)
        {
            new Server().Start();
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