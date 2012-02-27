using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Ponykart.Networking
{
    /// <summary>
    /// Represents the connection between a client and host across UDP.
    /// </summary>
    public class Connection
    {
        public int ConnectionID;
        public byte[] IDArray;
        public IPEndPoint DestinationEP;
        private DateTime LastSentTime, LastRecvTime;
        private UdpClient Sender;

        /// <summary>
        /// Creates a connection, given a destination to send to and a connection ID.
        /// </summary>
        public Connection(UdpClient sender, IPEndPoint destinationep, int cid)
        {
            DestinationEP = destinationep;
            ConnectionID = cid;
            Sender = sender;
            IDArray = System.BitConverter.GetBytes(ConnectionID);
        }


        public void Handle(byte[] packet) //TODO: Make Packet struct?
        {
            String contents = System.Text.ASCIIEncoding.ASCII.GetString(NetworkManager.GetContents(packet));
            Launch.Log(String.Format("Received packet: {0}", contents));
        }

        /// <summary>
        /// Creates a packet byte array.
        /// </summary>
        byte[] CreatePacket(int type, byte[] contents)
        {
            LastSentTime = System.DateTime.Now;
            var timeArr = System.BitConverter.GetBytes(LastSentTime.Ticks);
            var typeArr = System.BitConverter.GetBytes(LastSentTime.Ticks);
            var packet = new byte[contents.Length + 16];
            Array.Copy(NetworkManager.Protocol, packet, 4);
            Array.Copy(IDArray, 0, packet, 4, 4);
            Array.Copy(typeArr, 0, packet, 8, 4);
            Array.Copy(timeArr, 4, packet, 12, 4);
            return packet;
        }
        public bool SendPacket(byte[] contents)
        {
            var packet = CreatePacket(0, contents);
            Sender.Send(packet, packet.Length, DestinationEP);
            return true;
        }
    }
}
