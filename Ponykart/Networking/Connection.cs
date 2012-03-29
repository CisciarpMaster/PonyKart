using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Ponykart.Handlers;

namespace Ponykart.Networking {

	/// <summary>
	/// Represents the connection between a client and host across UDP.
	/// </summary>
	public class Connection {
		public Int32 ConnectionID;
		public byte[] IDArray;
		public IPEndPoint DestinationEP;
		private DateTime LastSentTime, LastRecvTime;
		private UdpClient Sender;
		private List<Message> Sent;
		public bool validated;

		/// <summary>
		/// Creates a connection, given a destination to send to and a connection ID.
		/// </summary>
		public Connection(UdpClient sender, IPEndPoint destinationep, int cid) {
			DestinationEP = destinationep;
			ConnectionID = cid;
			Sender = sender;
			IDArray = System.BitConverter.GetBytes(ConnectionID);
			Sent = new List<Message>();
		}

		/// <summary>
		/// Handle the information in a packet received via this connection.
		/// </summary>
		/// <param name="packet"></param>
		public void Handle(Packet packet) {
			string contents = System.Text.ASCIIEncoding.ASCII.GetString(packet.Contents);
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
							SendPacket((Int16) Commands.ConnectAccept, contents);
						}
						else {
							Launch.Log(string.Format("Client gave bad password: {0} instead of {1}", contents, nm.Password));
							SendPacket((Int16) Commands.ConnectReject, (string) null);
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
						LKernel.Get<MainMenuMultiplayerHandler>().LevelSelection = BitConverter.ToString(packet.Contents);
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
				default: // Unimplemented packet type
					Launch.Log(string.Format("Got unimplemented packet type: {0}", packet.Type));
					break;
			}
		}

		/// <summary>
		/// Creates a packet byte array.
		/// </summary>
		private byte[] PacketToBytes(Packet p) {
			var timeArr = BitConverter.GetBytes(p.Timestamp);
			var typeArr = BitConverter.GetBytes(p.Type);
			var packet = new byte[p.Contents.Length + 16];
			Array.Copy(NetworkManager.Protocol, packet, 4);
			Array.Copy(IDArray, 0, packet, 4, 4);
			Array.Copy(typeArr, 0, packet, 8, 2);
			Array.Copy(timeArr, 2, packet, 10, 6);
			Array.Copy(p.Contents, 0, packet, 16, p.Contents.Length);
			return packet;
		}

		private Packet CreatePacket(Message m) {
			Packet p = new Packet {
				CID = BitConverter.ToInt32(IDArray, 0),
				Contents = m.Contents,
				Protocol = NetworkManager.Protocol,
				Timestamp = System.DateTime.Now.Ticks,
				Type = m.Type
			};
			return p;
		}

		/// <summary>
		/// Sends a packet down this connection with the given contents and type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public bool SendPacket(Int16 type, string contents) {
			return SendPacket(type, System.Text.ASCIIEncoding.ASCII.GetBytes(contents));
		}
		public bool SendPacket(Commands type, string contents) {
			return SendPacket((short) type, System.Text.ASCIIEncoding.ASCII.GetBytes(contents));
		}
		/// <summary>
		/// Sends a packet down this connection with the given contents and type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public bool SendPacket(Int16 type, byte[] contents) {
			LastSentTime = System.DateTime.Now;
			var message = new Message(type, contents, LastSentTime.Ticks);
			Sent.Add(message);
			return SendPacket(message);
		}

		public bool SendPacket(Message message) {
			var packet = PacketToBytes(CreatePacket(message));
			if (Sender.Send(packet, packet.Length, DestinationEP) == 0) {
				return false;
			}
			else {
				return true;
			}
		}
		/// <summary>
		/// Resends any packets that have not received a reply
		/// </summary>
		public void ResendPackets() {
			var unresponded = from message in Sent
							  where (message.Responded == false)
							  select message;
			foreach (var message in unresponded) {
				SendPacket(message);
			}
		}
	}
}