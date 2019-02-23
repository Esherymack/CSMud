using System;
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

        // User property for the user's screen name
        public string User
        { get; set; }

        // a connection has a world
        public World World
        { get; }

        // Reader and Writer get expression-valued properties
        private StreamWriter Writer => new StreamWriter(new NetworkStream(socket, false));
        private StreamReader Reader => new StreamReader(new NetworkStream(socket, false));

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

            // this.LoopThread = new System.Threading.Thread(ClientLoop);
        }

        public void Start()
        {
            // Start a new Thread running ClientLoop()
            // LoopThread.Start();
            Task.Run(() => ClientLoop()).ContinueWith(t =>
                {
                    this.World.EndConnection(this);
                    socket.Close();
                    this.World.Broadcast($"{this.User} has disconnected.");
                    Console.WriteLine($"{this.User} has disconnected.");
                }
            );

        }

        //  ClientLoop basically handles the sending and receiving of messages as well as handling incoming and outgoing connections.
        void ClientLoop()
        {

            // Welcome the user to the game
            OnConnect();
            while (true)
            {
                // Get a message that's sent to the server
                string line = this.ReadMessage();
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
            this.World.Broadcast($"{this.User} has disconnected.");
            Console.WriteLine($"{this.User} has disconnected.");
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
            using (StreamWriter writer = this.Writer)
            {
                writer.WriteLine(line);
            }
        }        // SendMessage handles sending speech messages on the MUD server
        public string ReadMessage()
        {
            using (StreamReader reader = this.Reader)
            {
                return reader.ReadLine();
            }
        }
    }
}
