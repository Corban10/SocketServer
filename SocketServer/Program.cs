using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    class Program
    {
        // max size of message
        private byte[] buffer = new byte[1024];
        // list of open sockets
        private List<Socket> clientSockets = new List<Socket>();
        // server socket
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Program server = new Program();
            server.SetupServer();
        }
        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            // bind port 8080
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8080));
            // pending request backlog limit
            serverSocket.Listen(5);
            // begin accepting connections
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);

            // add socket to list of cllient sockets
            clientSockets.Add(socket);

            // open to receieving from client
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            // length of received bytes
            int recieved = socket.EndReceive(ar);
            // received bytes
            byte[] dataBuffer = new byte[recieved];

            Array.Copy(buffer, dataBuffer, recieved);

            // bytes as string
            string text = Encoding.ASCII.GetString(dataBuffer);
            Console.WriteLine($"Text received: {text}");

            string response = DateTime.Now.ToLongTimeString();
            if (text.ToLower() != "get time")
                response = "Invalid Request";

            // send current time to client
            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
    }
}
