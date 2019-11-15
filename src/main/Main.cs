/* CSMud - Rev 2
 * Author: Madison Tibbett
 * Last Modified: 05/14/2019
 */

using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSMud.Client;

namespace CSMud
{
    /* The Server class is a simple TCP server to accept connections
     * Server also holds the main function.
     */
    public class Server
    {
        // define static port number and backlog size
        const int port = 8088;
        const int backLog = 20;

        // a server has a world
        public World World
        { get; private set; }

        private readonly ManualResetEvent connectionEvent = new ManualResetEvent(false);

        // a ListenSocket is a socket
        private Socket ListenSocket
        { get; }

        // Constructor
        public Server()
        {
            // Create a new world on the server
            World = new World();
            /* 
            * Instantiate Socket object 'server'
            * 'AddressFamily.InterNetwork' refers to the AddressFamily enum - specifically addresses for IPv4
            * 'SocketType.Stream' refers to the SocketType enum - Stream supports two-way conn-based byte streams without duplication of data
            * 'ProtocolType.Tcp' refers to the ProtocolType enum - Tcp is a TCP server
            */
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /*
            * Socket.Bind method takes a local endpoint
            * IPEndPoint class represents a network endpoint as an IP address and port number
            */
            ListenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        // Start starts the server. 
        public void Start()
        {
            System.Console.WriteLine($"Listening on port {port}");
            /*
            * Socket.Listen method takes an integer and places the socket into a listening state. 
            * backLog is the maximum length of pending connections queue
            */
            ListenSocket.Listen(backLog);
            while(true)
            {
                connectionEvent.Reset();

                ListenSocket.BeginAccept(new System.AsyncCallback(AcceptConnection), ListenSocket);

                connectionEvent.WaitOne();
            }
        }

        private void AcceptConnection(System.IAsyncResult ar)
        {
            Socket clientSock = (ar.AsyncState as Socket).EndAccept(ar);
            connectionEvent.Set();
            World.NewConnection(clientSock);
        }

        // main just starts a new server 
        static void Main(string[] args) => new Server().Start();
    }
}