using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ponykart.Players;

namespace Ponykart.Networking
{
    /// <summary>
    /// This class manages Network connections
    /// </summary>
    public class NetworkManager
    {
        public static readonly byte[] Protocol = { 123, 10, 205, 7 };
        public static readonly int HOST = 0;
        public static readonly int CLIENT = 0;

        // For receiving connections from the other party
        private UdpClient Listener;
        private IPEndPoint ListenEP;
        // For sending information. Host has many, Client has one.
        private IDictionary<int,Connection> Connections;
        public Connection SingleConnection;
        // Password for connection
        private String Password;

        // What kind of Networking connection we have
        private int NetworkType = 0;

        // Networking thread 
        public static Thread NetworkingThread;
        
        private Player LocalPlayer { get; set; }
        private Player[] NetPlayers { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManager() {
        }
        
        /// <summary>
        /// Initializes as Host
        /// </summary>
        /// <param name="port">Port to listen on</param>
        /// <param name="password">String password</param>
        public void InitManager(int port, String password)
        {
            NetworkType = HOST;
            Password = password;
            Listener = new UdpClient(port);
            ListenEP = new IPEndPoint(IPAddress.Any, port);
            Connections = new Dictionary<int,Connection>();
        }

        /// <summary>
        /// Initializes as Client
        /// </summary>
        /// <param name="port">Port to connect on. Trusted to be valid.</param>
        /// <param name="ip">IP (TRUSTED!! To be valid)</param>
        public void InitManager(int port, String password, String ip)
        {
            NetworkType = CLIENT;
            Password = password;
            Listener = new UdpClient(port); //Todo: Add checks
            ListenEP = new IPEndPoint(IPAddress.Parse(ip), port);//Todo: Add checks
            SingleConnection = new Connection(Listener, ListenEP, 0); 
        }

        /// <summary>
        /// Strips out header information from a packet and returns the contents
        /// </summary>
        public static byte[] GetContents(byte[] packet)
        {
            var contents = new byte[packet.Length - 16];
            Array.Copy(packet, 16, contents, 0, packet.Length - 16);
            return contents;
        }

        /// <summary>
        /// Get specific header information from a packet
        /// </summary>
        public static byte[] GetProtocol(byte[] packet) 
        {
            var protocol = new byte[4];
            Array.Copy(packet, protocol, 4);
            return protocol;
        }
        public static int GetID(byte[] packet)
        {
            return BitConverter.ToInt32(packet,4);
        }

        /// <summary>
        /// Called every time we receive a new packet.
        /// </summary>
        public void OnPacket(byte[] packet)
        {

            if (GetProtocol(packet).Equals(Protocol))
            {
                if (NetworkType == CLIENT)
                {
                    if (GetID(packet).Equals(SingleConnection.ConnectionID))
                    {
                        SingleConnection.Handle(packet);
                    }
                }
                else if (NetworkType == HOST)
                {
                    int id = GetID(packet);
                    if (Connections.ContainsKey(id))
                    {
                        Connections[id].Handle(packet);
                    }
                }

            }
        }

        /// <summary>
        /// Starts up the listener thread.
        /// </summary>
        /// <param name="nConnections">Maximum concurrent connections</param>
        public static void StartThread(int nConnections) 
        {
            NetworkManager m = LKernel.Get<NetworkManager>();
            Thread t = new Thread(m.AcceptConnections);
            Launch.Log("Network thread opened.");
            NetworkManager.NetworkingThread = t;
            t.Start(nConnections);
        }

        /// <summary>
        /// Kill network thread.
        /// </summary>
        public static void StopThread()
        {
            Launch.Log("Network thread halting...");
            NetworkManager.NetworkingThread.Abort();
            NetworkManager.NetworkingThread.Join();
            Launch.Log("Network thread joined.");
        }

        /// <summary>
        /// Main loop for receiving packets and creating connections.
        /// </summary>
        /// <param name="data">integer, contains the number of concurrent connections to allow</param>
        void AcceptConnections(object data) {
            int nConnections = (int)data;
            try
            {
                while (true)
                {
                    if (Listener.Available > 0)
                    {
                        OnPacket(Listener.Receive(ref ListenEP));
                    }
                }

            }
            catch (ThreadAbortException tae)
            {
                Launch.Log("Networking thread aborted");
            }
        }
    }
}
