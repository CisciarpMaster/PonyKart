using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Ponykart.Players;

namespace Ponykart.Networking
{  

     public enum Commands {
        Connect = 0x0000,
        ConnectAccept = 0x0001,
        ConnectReject = 0x0002,
        SelectLevel = 0x1000,
        LevelAccept = 0x1001,
        CharacterSelect = 0x1010,
        CharacterTaken = 0x1011,
        Characters = 0x1100,
        StartGame = 0x1F00,
        StartAccept = 0x1F01,
        ServerMessage = 0xF000,
        NoMessage = 0xFFFF // empty message only to send acks
    };

    /// <summary>
    /// This class manages Network connections
    /// </summary>
    public class NetworkManager
    {
        public static readonly byte[] Protocol = { 123, 10, 205, 7 };
        public static readonly int HOST = 0;
        public static readonly int CLIENT = 1;

        // For receiving connections from the other party
        private UdpClient Listener;
        private IPEndPoint ListenEP;
        // For sending information. Host has many, Client has one.
        private IDictionary<int, Connection> Connections;
        public Connection SingleConnection;
        // Password for connection
        public string Password;

        // What kind of Networking connection we have
        public int NetworkType;

        // Keep track of how many concurrent connections we allow
        private int MaxConnections;

        // Networking thread 
        public Thread NetworkingThread;

        // Speed to send packets at
        public int PacketsPerSecond = 30;

        // Time last packet was sent
        long LastSentTicks;
        
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
        /// <param name="password">string password</param>
        public void InitManager(int port, string password) {
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
        public void InitManager(int port, string password, string ip)
        {
            NetworkType = CLIENT;
            Password = password;
            Listener = new UdpClient(port); //Todo: Add checks
            ListenEP = new IPEndPoint(IPAddress.Parse(ip), port); //Todo: Add checks
            SingleConnection = new Connection(Listener, ListenEP, 0);
            Connections = new Dictionary<int, Connection>();
            Connections.Add(0, SingleConnection);
        }
        /// <summary>
        /// Called every time we receive a new packet.
        /// </summary>
        public void OnPacket(byte[] packet) {
           // Launch.Log("Processing packet.");
            UDPPacket p = new UDPPacket(packet);
    //        Launch.Log(string.Format("Protocol string: {0}. Ours: {1}", System.Text.ASCIIEncoding.ASCII.GetString(p.Protocol),
    //                                                                   System.Text.ASCIIEncoding.ASCII.GetString(Protocol)));
            if (p.Protocol.SequenceEqual(Protocol)) {
               // Launch.Log("Packet of our protocol.");
                if (NetworkType == CLIENT) {
                  //  Launch.Log("Received packet as client");
                    if (p.CID == SingleConnection.UDPConnection.ConnectionID) {
                        SingleConnection.UDPConnection.Handle(p);
                    }
                }
                else if (NetworkType == HOST) {
                  //  Launch.Log("Received packet as host");
                    int id = (int)p.CID;
                    if (Connections.ContainsKey(id)) {
                        Connections[id].UDPConnection.Handle(p);
                    } else if (Connections.Keys.Count < MaxConnections) {
                        Launch.Log("Allowing new connection");
                        Connections.Add(id, new Connection(Listener, new IPEndPoint(ListenEP.Address, ListenEP.Port), id));
                        Connections[id].UDPConnection.Handle(p);
                    }

                }

            }
        }

        /// <summary>
        /// Starts up the listener thread.
        /// </summary>
        /// <param name="nConnections">Maximum concurrent connections</param>
        public void StartThread(int nConnections) {
            Thread t = new Thread(AcceptConnections);
            Launch.Log("Network thread opened.");
            NetworkingThread = t;
            t.Start(nConnections);
        }

        /// <summary>
        /// Kill network thread.
        /// </summary>
        public void StopThread() {
            Launch.Log("Network thread halting...");
            NetworkingThread.Abort();
            NetworkingThread.Join();
            Launch.Log("Network thread joined.");
        }

        /// <summary>
        /// Main loop for receiving packets and creating connections.
        /// </summary>
        /// <param name="data">integer, contains the number of concurrent connections to allow</param>
        void AcceptConnections(object data) {
            int nConnections = (int)data;
            MaxConnections = nConnections;
            Launch.Log("waiting for connections...");
            LastSentTicks = 0;
            try {
                while (!Launch.Quit) {
                    if (Listener.Available > 0) {
                        Launch.Log("Packet available");
                        OnPacket(Listener.Receive(ref ListenEP));
                    }
                    Launch.Log(String.Format("Seconds since last: {0}; Ticks: {1}", new TimeSpan(PacketsPerSecond*(System.DateTime.Now.Ticks - LastSentTicks)).Seconds,
                        PacketsPerSecond * (System.DateTime.Now.Ticks - LastSentTicks)));
                    if (new TimeSpan((System.DateTime.Now.Ticks - LastSentTicks) * PacketsPerSecond).Seconds > 1)  {
                        LastSentTicks = System.DateTime.Now.Ticks;
                        ForEachUDPConnection(udpc => udpc.Send());
                    }
                    ForEachUDPConnection(udpc => udpc.ResendPackets());
                }

            } catch (ThreadAbortException) {
                Launch.Log("Networking thread aborted");
            }
        }

        public void ForEachConnection(Action<Connection> act) {
            foreach (var c in Connections.Values) {
                act(c);
            }
        }
        public void ForEachUDPConnection(Action<ReliableUDPConnection> act) {
            foreach (var c in Connections.Values) {
                act(c.UDPConnection);
            }
        }

        public void CloseConnection(Connection connection) {
            Connections.Remove(connection.Cid);
        }
    }
}