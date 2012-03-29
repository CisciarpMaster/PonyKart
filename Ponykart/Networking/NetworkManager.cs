using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ponykart.Players;

namespace Ponykart.Networking
{  
    public struct Packet {
        public byte[] Protocol;
        public Int32 CID;
        public Int64 Timestamp;
        public Int16 Type;
        public byte[] Contents;
        public Message Information;
        /// <summary>
        /// Get specific header information from a packet
        /// </summary>
        public static byte[] GetProtocol(byte[] packet)  {
            var protocol = new byte[4];
            Array.Copy(packet, protocol, 4);
            return protocol;
        }
        public static Int32 GetID(byte[] packet) {
            return BitConverter.ToInt32(packet,4);
        }
        public static Int64 GetTime(byte[] packet) {
            return (((Int64)BitConverter.ToInt32(packet,10))<<2) +
                    BitConverter.ToInt16(packet,14);
        }
        public static Int16 GetType(byte[] packet) {
            return BitConverter.ToInt16(packet, 8);
        }

        /// <summary>
        /// Strips out header information from a packet and returns the contents
        /// </summary>
        public static byte[] GetContents(byte[] packet)  {
            var contents = new byte[packet.Length - 16];
            Array.Copy(packet, 16, contents, 0, packet.Length - 16);
            return contents;
        }


        public Packet(byte[] creator) {
            this.Protocol = Packet.GetProtocol(creator);
            this.CID = Packet.GetID(creator);
            this.Timestamp = Packet.GetTime(creator);
            this.Contents = Packet.GetContents(creator);
            this.Type = Packet.GetType(creator);
            this.Information = new Message(this.Type, this.Contents, this.Timestamp);

        }
    }

    /// <summary>
    /// Struct to help keep track of packets that have and have not been replied to
    /// </summary>
    public struct Message {
        public byte[] Contents;
        public Int16 Type;
        public Int16[] Responses;
        public Int64 Timestamp;
        public bool Responded;

        public Message(Int16 type, byte[] contents, Int64 timestamp) {
            // TODO: Complete member initialization
            this.Type = type;
            this.Contents = contents;
            this.Timestamp = timestamp;
            this.Responses = new Int16[0];
            this.Responded = false;
        }
    }

    public enum Commands {
        Connect = 0x0000,
        ConnectAccept = 0x0001,
        ConnectReject = 0x0002,
        SelectLevel = 0x1000,
        LevelAccept = 0x1001,
        StartGame = 0x1F00,
        StartAccept = 0x1F01,
        ServerMessage = 0xF000
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
        }
        /// <summary>
        /// Called every time we receive a new packet.
        /// </summary>
        public void OnPacket(byte[] packet) {
            Launch.Log("Processing packet.");
            Packet p = new Packet(packet);
            Launch.Log(string.Format("Protocol string: {0}. Ours: {1}", System.Text.ASCIIEncoding.ASCII.GetString(p.Protocol),
                                                                        System.Text.ASCIIEncoding.ASCII.GetString(Protocol)));
            if (p.Protocol.SequenceEqual(Protocol)) {
                Launch.Log("Packet of our protocol.");
                if (NetworkType == CLIENT) {
                    Launch.Log("Received packet as client");
                    if (p.CID  == SingleConnection.ConnectionID) {
                        SingleConnection.Handle(p);
                    }
                }
                else if (NetworkType == HOST) {
                    Launch.Log("Received packet as host");
                    int id = p.CID;
                    if (Connections.ContainsKey(id)) {
                        Connections[id].Handle(p);
                    } else if (Connections.Keys.Count < MaxConnections) {
                        Launch.Log("Allowing new connection");
                        Connections.Add(id, new Connection(Listener, new IPEndPoint(ListenEP.Address, ListenEP.Port), id));
                        Connections[id].Handle(p);
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
            try {
                while (!Launch.Quit) {
                    if (Listener.Available > 0) {
                        Launch.Log("Packet available");
                        OnPacket(Listener.Receive(ref ListenEP));
                    }
                    // TODO: Check each connection for un-answered packets
                    foreach (var c in Connections.Values) {
                        c.ResendPackets();
                    }
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
    }
}