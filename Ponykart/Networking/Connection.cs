using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Ponykart.Handlers;
using Ponykart.Players;

namespace Ponykart.Networking {

	/// <summary>
	/// Represents the connection between a client and host across UDP.
	/// </summary>
	public class Connection {
		private DateTime LastSentTime, LastRecvTime;
        public long ZeroMoment,RemoteOffset;
		public bool validated;
        public readonly ReliableUDPConnection UDPConnection;
        public readonly int Cid;
        Queue<Message> OutgoingQueue;

        /// <summary>
        /// Gets the top message on the queue, then removes it (!). 
        /// Generates an empty message if there are none waiting.
        /// </summary>
        public Message TopMessage {
            get {
                if (OutgoingQueue.Count > 0) {
                    return OutgoingQueue.Dequeue();
                } else {
                    return new Message(Commands.NoMessage, "", true);
                }
            }
        }

		/// <summary>
		/// Creates a connection, given a destination to send to and a connection ID.
		/// </summary>
		public Connection(UdpClient sender, IPEndPoint destinationep, Int32 cid) {
            UDPConnection = new ReliableUDPConnection(sender, destinationep, cid, this);
            OutgoingQueue = new Queue<Message>();
            Cid = cid;
		}

		/// <summary>
		/// Handle the information in a packet received via this connection.
		/// </summary>
		/// <param name="packet"></param>
		public void Handle(PonykartPacket packet) {
            LastRecvTime = System.DateTime.Now;
            string contents = packet.StringContents;
			Launch.Log(string.Format("Received packet type {1}: {0}", contents, packet.Type));
			NetworkManager nm = LKernel.Get<NetworkManager>();
			if (!Enum.IsDefined(typeof(Commands), (Commands) packet.Type)) {
				// Unrecognized packet type!
				Launch.Log(string.Format("Error: Malformed packet type: {0}", packet.Type));
				return;
			}
			switch ((Commands) packet.Type) {
#region Connection Negotiation
				case Commands.Connect:
					if (nm.NetworkType == NetworkManager.CLIENT) {
						return;
					}
					else {
						if (nm.Password.SequenceEqual(contents)) {
							Launch.Log(string.Format("Client provided correct password ({0})", contents));
                            validated = true;
							SendPacket((Int16) Commands.ConnectAccept, contents);
						}
						else {
							Launch.Log(string.Format("Client gave bad password: {0} instead of {1}", contents, nm.Password));
							SendPacket((Int16) Commands.ConnectReject, (string) null);
                            CloseConnection();
						}
					}
					break;
				case Commands.ConnectAccept:
					Launch.Log("Server accepted our password.");
					break;
				case Commands.ConnectReject:
					Launch.Log("Server rejected our password.");
					break;
#endregion
#region Pre-game Setup
				case Commands.SelectLevel:
					if (nm.NetworkType == NetworkManager.CLIENT) {
						LKernel.Get<MainMenuMultiplayerHandler>().LevelSelection = packet.StringContents;
						SendPacket(Commands.LevelAccept, "");
					}
					break;
				case Commands.StartGame:
					if (nm.NetworkType == NetworkManager.CLIENT) {
						LKernel.Get<MainMenuMultiplayerHandler>().Start_Game();
					}
					break;
#endregion
				case Commands.ServerMessage:
					Launch.Log(string.Format("Server message: '{0}'", contents));
					break;
                case Commands.NoMessage: // Do nothing.
                    break;
				default: // Unimplemented packet type
					Launch.Log(string.Format("Got unimplemented packet type: {0}", packet.Type));
					break;
			}
		}
		/// <summary>
		/// Sends a packet down this connection with the given contents and type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public void SendPacket(Int16 type, string contents, bool isVolatile=false) {
			SendPacket((Commands)type, contents, isVolatile);
		}
		public void SendPacket(Commands type, string contents, bool isVolatile=false) {
			SendPacket(type, System.Text.ASCIIEncoding.ASCII.GetBytes(contents??""),isVolatile);
		}
        public void SendPacket(Commands type, byte[] contents, bool isVolatile=false) {
            LastSentTime = System.DateTime.Now;
            //var message = new UDPPacket(new PonykartPacket(type, contents, this), UDPConnection);
            var message = new Message(type, contents, isVolatile);
            Launch.Log(String.Format("Queued outgoing packet of type {0}", type));
            OutgoingQueue.Enqueue(message);
        }

        public void CloseConnection() {
            UDPConnection.Close();
            LKernel.Get<NetworkManager>().CloseConnection(this);
        }
    }
}