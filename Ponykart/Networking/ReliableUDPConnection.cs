using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace Ponykart.Networking {
    public class ReliableUDPConnection {
        private UdpClient Sender;
        public UInt32 ConnectionID;
        public byte[] IDArray;
        public IPEndPoint DestinationEP;
        private Dictionary<UInt32,UDPPacket> Sent;
        public UInt32 RemoteSeqNo = 0;
        public UInt32 AckField = 0;
        int sequenceNo = 0;
        long LastReceivedTicks = 0;
        Connection Owner;

        public ReliableUDPConnection(UdpClient sender, IPEndPoint destinationep, UInt32 cid, Connection owner) {
            DestinationEP = destinationep;
            ConnectionID = cid;
            Sender = sender;
            Sent = new Dictionary<UInt32,UDPPacket>();
            Owner = owner;
        }

        public void Handle(UDPPacket p) {
            AddAck(p);
            ProcessAcks(p.Ack, p.AckField);
            Owner.Handle(p.Contents);
        }
            
        /// <summary>
        /// Adds a packet that has been received to the next ack that will be sent
        /// </summary>
        /// <param name="p"></param>
        void AddAck(UDPPacket p) {
            LastReceivedTicks = System.DateTime.Now.Ticks;
            // evil bit-field hacking. TODO: explain?
            if (p.SequenceNo > RemoteSeqNo) {
                AckField <<= (int)(p.SequenceNo - RemoteSeqNo);
            }
            AckField |= (UInt32)(1 << (int)(RemoteSeqNo - p.SequenceNo));
            if (p.SequenceNo >= RemoteSeqNo) {
                RemoteSeqNo = p.SequenceNo;
            }
        }

        /// <summary>
        /// Reads the acks from a packet and notes which packets have been received
        /// </summary>
        /// <param name="Ack"></param>
        /// <param name="AckField"></param>
        void ProcessAcks(UInt32 Ack, UInt32 AckField) {
            // evil bit-field hacking. TODO: explain?
            for (int i = 0; i < 32; i++) {
                if (((int)AckField & 1) == 1) {
                    if (Sent.ContainsKey((UInt32)(Ack - i))) {
                        var packet = Sent[(UInt32)(Ack - i)];
                        if (packet.Contents.Type != Commands.NoMessage && !packet.Responded)  {
                            Launch.Log(String.Format("Remote partner received packet {0} type {1}", packet.SequenceNo,
                               packet.Contents.Type));
                        }
                        packet.Responded = true;
                    }
                }
                AckField >>= 1;
            }
        }


        public UInt32 NextSequenceNumber {
            get {
                return (UInt32)sequenceNo++;
            }
        }
        bool SendPacket(UDPPacket packet) {
            packet.LastSent = DateTime.Now;
            var bytes = packet.ToBytes();
            if (Sender.Send(bytes, bytes.Length, DestinationEP) == 0) {
                return false;
            } else {
                //Launch.Log(String.Format("Sent message id {0} type {1}", packet.SequenceNo, packet.Contents.Type));
                return true;
            }
        }

        /// <summary>
        /// Resends any packets that have not received a reply
        /// </summary>
        public void ResendPackets() {
            var unresponded = from message in Sent.Values
                              where !message.Responded
                              where (DateTime.Now - message.LastSent) > TimeSpan.FromSeconds(5)
                              select message;
            foreach (var message in unresponded) {
                message.SequenceNo = NextSequenceNumber;
                SendPacket(message);
            }
        }

        public void AddPacket(UDPPacket message) {            
            Sent[message.SequenceNo] = message;
        }

        public void Send() {
            var TopPacket = Owner.TopMessage.ToPKPacket(Owner);
            var message = new UDPPacket(TopPacket,this);
            if (!TopPacket.Volatile) {
                AddPacket(message);
            }
            SendPacket(message);
        }

        public void Close() {
            Sender.Close();
        }
    }
}
