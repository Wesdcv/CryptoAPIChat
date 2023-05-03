using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static System.Collections.Specialized.BitVector32;
using RSA2048Sharp;

namespace Server
{
    public class Session
    {
        private Server server;
        private int SessionID;
        private TcpClient client;
        private BinaryReader reader;
        private BinaryWriter writer;
        public Session(TcpClient client, BinaryReader reader, BinaryWriter writer, Server server, int SessionID)
        {
            this.client = client;
            this.reader = reader;
            this.writer = writer;
            this.server = server;
            this.SessionID = SessionID;
        }

        public int GetSessionID()
        {
            return SessionID;
        }

        public void StartSession()
        {    
            Thread thread = new Thread(HandleSession);
            thread.Start();
        }

        private void HandleSession()
        {
            Console.WriteLine("Client {0} Session...", SessionID);
            writer.Write("You are connected.");
            while (true)
            {
                try
                {
                    string message = reader.ReadString();
                    BroadcastMessage(message);

                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                    server.GetClients().Remove(this);
                    Console.WriteLine("Session has been closed");
                    server.NotifyAboutClose(SessionID);
                    break;
                }
            }
        }
        private void BroadcastMessage(string message)
        {
            server.BroadcastMessage(SessionID, message);
        }
        public void SendMessage(string message)
        {
            writer.Write(message);
        }
    }
    public class Server
    {
        private TcpListener listener;
        private List<Session> clients;
        public Server(string IpAddress, int port)
        {
            listener = new TcpListener(IPAddress.Parse(IpAddress), port);
            clients = new List<Session>();
        }

        public TcpListener GetListener()
        {
            return this.listener;
        }
        public List<Session> GetClients()
        {
            return this.clients;
        }

        public void NotifyAboutClose(int sessionID)
        {
            Console.WriteLine($"Client {sessionID} has been disconnected.");
            BroadcastMessage(sessionID, " has been disconnected.");
        }

        public void BroadcastMessage(int SessionID, string message)
        {
            foreach (Session session in clients)
            {
                if (session.GetSessionID() != SessionID)
                {
                    session.SendMessage($"{message}"); // session.SendMessage($"[Client{SessionID}]:{message}");
                }
            }
        }

        public static void Main(string[] args)
        {
            RSA384.GenerateKeys();
            Console.Write("IP Address: ");
            string ipAddress = Console.ReadLine();
            Console.Write("Port: ");
            int port = Int32.Parse(Console.ReadLine());
            Server server = new Server(ipAddress, port);
            server.GetListener().Start();
            Console.WriteLine("Server is listening for new clients...");
            while (true)
            {
                TcpClient newClient = server.GetListener().AcceptTcpClient();
                BinaryReader reader = new BinaryReader(newClient.GetStream());
                BinaryWriter writer = new BinaryWriter(newClient.GetStream());
                Session newSession = new Session(newClient, reader, writer, server, server.GetClients().Count);
                newSession.StartSession();
                server.GetClients().Add(newSession);
                Console.WriteLine("Connected clients: {0}", server.GetClients().Count);
            }
        }
    }
}