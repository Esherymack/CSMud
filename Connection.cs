﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CSMud
{
    /* Connection is basically the "client" - you don't have to have a separate client program
 * this decision was largely due to all the other MUDs out there being Telnet connections
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
        // private System.Threading.Thread LoopThread
        // { get; set; }

        private System.Threading.CancellationTokenSource CTSource
        { get; set; }

        private NetworkStream NStream
        { get; set; }

        // User property for the user's screen name
        public string User
        { get; set; }

        // a connection has a world
        public World World
        { get; }

        // Reader and Writer get expression-valued properties
        private StreamWriter Writer => new StreamWriter(this.NStream);
        private StreamReader Reader => new StreamReader(this.NStream);

        // Constructor
        public Connection(Socket socket, World world)
        {
            this.socket = socket;
            this.World = world;
            this.NStream = new NetworkStream(socket, false)
            {
                //ReadTimeout = 10000
            };
            /* Every connection gets a reader and writer
            * the writer is set to auto-flush for every user - this helps get messages displayed properly to each individual user
            * Get the user's screen name for later use
            */
            GetLogin();

            // this.LoopThread = new System.Threading.Thread(ClientLoop);
        }

        public void Start()
        {
            // Start a new Thread running ClientLoop()
            // LoopThread.Start();
            this.CTSource = new System.Threading.CancellationTokenSource();
            Task.Run(() => ClientLoop(), CTSource.Token);
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
                        // TODO : Move the "quit" condition to a command rather than something that's checked here
                        if (line == null || line == "quit")
                        {
                            break;
                        }
                        // otherwise, we process the line
                        else
                        {
                            string message = FormatMessage(line);
                            SendMessage(message);
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
            this.SendMessage("Please enter a name: ");
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
            // multiline string literal is making me very upset, many conniptions
            this.SendMessage(@"Welcome!
Send 'quit' to exit.
Send 'help' for help.");
        }

        // OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
        void OnDisconnect()
        {
            this.World.EndConnection(this);
            socket.Close();
            this.CTSource.Cancel();
        }

        /*
         * ProcessLine handles organizing messages on the MUD server
         * as of right now it just trims and sends the message to SendMessasge
         */
        string FormatMessage(string line)
        {
            return $"{this.User} says, '{line.Trim()}'";
        }

        // SendMessage handles sending speech messages on the MUD server
        public void SendMessage(string line)
        {
            try
            {
                using (StreamWriter writer = this.Writer)
                {
                    writer.WriteLine(line);
                }
            }
            catch (IOException e) when (e.InnerException is SocketException)
            {
                OnDisconnect();
            }
        }        // SendMessage handles sending speech messages on the MUD server
        public string ReadMessage()
        {
            try
            {
                using (StreamReader reader = this.Reader)
                {
                    return reader.ReadLine();
                }
            }
            catch (IOException e) when (e.InnerException is SocketException)
            {
                OnDisconnect();
                return null;
            }
        }
    }
}