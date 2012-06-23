using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Ponykart.Actors;
using Ponykart.Handlers;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Networking;

namespace Ponykart.Networking {
	/// <summary>
	/// Represents the connection between a client and host across UDP.
	/// </summary>
	public class Connection {
		private DateTime LastSentTime, LastRecvTime;
        public long ZeroMoment,RemoteOffset;
		public bool validated;
        public readonly ReliableUDPConnection UDPConnection;
        public readonly UInt32 Cid;
        public int PlayerCt = 1;
        Queue<Message> OutgoingQueue;
        NetworkManager nm;
        List<KartInformation> KartsToSend;
        public IEnumerable<NetworkEntity> Players {
            get {
                return (from ent in LKernel.Get<NetworkManager>().Players
                        where ent.owner == this
                        select ent);
            }
        }

        string PrepareKarts() {
            return "";
        }

        /// <summary>
        /// Gets the top message on the queue, then removes it (!). 
        /// Generates an empty message if there are none waiting.
        /// </summary>
        public Message TopMessage {
            get {
                if (OutgoingQueue.Count > 0) {
                    return OutgoingQueue.Dequeue();
                } else {
                    if (nm.GameRunning) {
                        return new Message(Commands.SendPositions, nm.SerializeKarts(), true);
                    }
                    return new Message(Commands.NoMessage, "", true);
                }
            }
        }

		/// <summary>
		/// Creates a connection, given a destination to send to and a connection ID.
		/// </summary>
		public Connection(UdpClient sender, IPEndPoint destinationep, UInt32 cid) {
            UDPConnection = new ReliableUDPConnection(sender, destinationep, cid, this);
            OutgoingQueue = new Queue<Message>();
            Cid = cid;
            nm = LKernel.GetG<NetworkManager>();
		}

		/// <summary>
		/// Handle the information in a packet received via this connection.
		/// </summary>
		/// <param name="packet"></param>
		public void Handle(PonykartPacket packet) {
            LastRecvTime = System.DateTime.Now;
            string contents = packet.StringContents;
            byte[] contentsArr = packet.ToBytes();
			//Launch.Log(string.Format("Received packet type {1}: {0}", contents, packet.Type));
			NetworkManager nm = LKernel.Get<NetworkManager>();
			if (!Enum.IsDefined(typeof(Commands), (Commands) packet.Type)) {
				// Unrecognized packet type!
                Launch.Log(string.Format("Error: Malformed packet type: {0}", packet.Type)); 
				return;
			}
			switch ((Commands) packet.Type) {
#region Connection Negotiation
				case Commands.Connect:
					if (nm.NetworkType == NetworkTypes.Client) {
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
#region Player Management
                case Commands.RequestPlayer :
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (nm.Players.Count < Settings.Default.NumberOfPlayers) {
                            var NPlayer = new NetworkEntity(this);
                            nm.Players.Add(NPlayer);
                            nm.ForEachConnection((c) => c.SendPacket(Commands.NewPlayer, NPlayer.Serialize()));
                        } else {
                            // TODO: Reject player
                        }
                    }
                    break;

                case Commands.NewPlayer :
                    if (nm.NetworkType == NetworkTypes.Client) {
                        nm.Players.Add(NetworkEntity.Deserialize(packet.StringContents,this));
                    }
                    break;

                case Commands.RejectPlayer :
                    // TODO: move forward in menus only when applicable
                    break;

                case Commands.RequestPlayerChange :
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (NetworkEntity.PerformChange(packet.StringContents, this)) {
                            nm.ForEachConnection((c) => c.SendPacket(Commands.PlayerChange, packet.StringContents));
                        } else { 
                            this.SendPacket(Commands.RejectChange,packet.StringContents); // TODO: include reason?
                        }
                    }
                    break;
                case Commands.PlayerChange :
                    if (nm.NetworkType == NetworkTypes.Client) {
                        if (!NetworkEntity.PerformChange(packet.StringContents, this)) {
                            // TODO: Request new player list.
                        }
                    }
                    break;

                case Commands.LeaveGame:
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (!NetworkEntity.PerformChange(packet.StringContents, this)) {
                            // TODO: Request new player list.
                        }
                    }
                    break;

                case Commands.RemovePlayer:
                    Launch.Log(string.Format("Got unimplemented packet type: {0}", packet.Type));

                    break;

#endregion
                #region Pre-game Setup
                case Commands.SelectLevel:
					if (nm.NetworkType == NetworkTypes.Client) {
						LKernel.Get<MainMenuMultiplayerHandler>().LevelSelection = packet.StringContents;
						SendPacket(Commands.LevelAccept, "");
					}
					break;
				case Commands.StartGame:
					if (nm.NetworkType == NetworkTypes.Client) {
						LKernel.Get<MainMenuMultiplayerHandler>().Start_Game();
					}
					break;
#endregion
                case Commands.SendPositions:
                    if (nm.NetworkType == NetworkTypes.Client) {
                        // TODO: implement. 
                    } else {
                    }
                    break;
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
		public void SendPacket(Commands type, string contents="", bool isVolatile=false) {
			SendPacket(type, System.Text.ASCIIEncoding.ASCII.GetBytes(contents??""),isVolatile);
		}
        public void SendPacket(Commands type, byte[] contents, bool isVolatile=false) {
            LastSentTime = System.DateTime.Now;
            //var message = new UDPPacket(new PonykartPacket(type, contents, this), UDPConnection);
            var message = new Message(type, contents, isVolatile);
            Launch.Log(String.Format("Queued outgoing packet of type {0}", type));
            OutgoingQueue.Enqueue(message);
        }

        public void SetKarts() {
            PlayerManager pm = LKernel.GetG<PlayerManager>();
            NetworkManager nm = LKernel.GetG<NetworkManager>();
            if(nm.NetworkType == NetworkTypes.Client) {
                KartsToSend = (from p in pm.Players where p.IsLocal select nm.Karts[p.Kart]).ToList<KartInformation>();
            } else {}
        }

        public void CloseConnection() {
            UDPConnection.Close();
            LKernel.Get<NetworkManager>().CloseConnection(this);
        }
    }
}