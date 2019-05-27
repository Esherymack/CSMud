using System;
using System.IO;
using System.Net.Sockets;

namespace CSMud.Client
{
    public class Connection : IDisposable
    {
        private readonly object streamWrite = new object();
        private readonly object streamRead = new object();
        private NetworkStream SockStream { get; }

        // Reader and Writer get expression-valued properties
        private StreamWriter Writer => new StreamWriter(SockStream, System.Text.Encoding.ASCII, 1024, true);
        private StreamReader Reader => new StreamReader(SockStream, System.Text.Encoding.ASCII, false, 1024, true);

        public Connection(Socket socket)
        {
            SockStream = new NetworkStream(socket, true);
        }

        // SendMessage handles sending messages on the MUD server
        public void SendMessage(string line)
        {
            lock (streamWrite)
            {
                using (StreamWriter writer = Writer)
                {
                    writer.WriteLine(line);
                }
            }
        }

        // ReadMessage handles reading speech messages on the MUD server
        public string ReadMessage()
        {
            lock (streamRead)
            {
                using (StreamReader reader = Reader)
                {
                    return reader.ReadLine();
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)SockStream).Dispose();
        }
    }
}