using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Ponykart.Players;

namespace Ponykart.Networking {
    public class NetworkEntity {
        public Player player;
        public bool local;
        NetworkManager nm;
        private int _GlobalID;

        /// <summary>
        /// Set this to change the global id directly
        /// </summary>
        public int GlobalID {
            get {
                return _GlobalID;
            }
            set {
                _GlobalID = value;
            }
        }
        private string _Name;
        /// <summary>
        /// Set this to change the name directly
        /// </summary>
        public string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }
        private string _Selection;
        /// <summary>
        /// Set this to change the selection directly
        /// </summary>
        public string Selection {
            get {
                return _Selection;
            }
            set {
                _Selection = value;
            }
        }
        public Connection owner;

        public NetworkEntity(Connection parent, int globalid, string name, string selection, bool islocal) {
            if (parent != null) { owner = parent; }
            local = islocal;
            _GlobalID = globalid;
            _Selection = selection;
            _Name = name;
            nm = LKernel.Get<NetworkManager>();
        }

        public NetworkEntity(Connection parent) {
            local = false;
            owner = parent;
            nm = LKernel.Get<NetworkManager>();
            _GlobalID = nm.AssignGlobalID();
            _Name = String.Format("Ponefag{0}", _GlobalID);
            _Selection = "Twilight Sparkle";
        }
        /// <summary>
        /// Use to attempt to set the name among all instances.
        /// </summary>
        /// <param name="name">The new name</param>
        public void SetName(string name) {
            if (nm.NetworkType == NetworkTypes.Client) {
                owner.SendPacket(Commands.RequestPlayerChange, SerializeChange("Name",name));
            } else {
                _Name = name;
                nm.ForEachConnection((c) => c.SendPacket(Commands.PlayerChange, SerializeChange("Name", name)));
            }
        }

        /// <summary>
        /// Turn this NetworkEntity into a string that can be sent to a client
        /// </summary>
        /// <returns></returns>
        public string Serialize() {
            var SWriter = new StringWriter();
            var XWriter = new XmlTextWriter(SWriter);
            
            var AsXML = new XElement("Entity", new XAttribute("Name",Name),
                new XAttribute("Id",_GlobalID), new XAttribute("Selection", _Selection));

            AsXML.WriteTo(XWriter);
            return SWriter.ToString();
        }

        /// <summary>
        /// Turn a string representing a player into a NetworkEntity
        /// </summary>
        /// <param name="contents">The XML tree representing the entity</param>
        /// <param name="parent">The connection that received this tree</param>
        /// <returns>A new NetworkEntity</returns>
        public static NetworkEntity Deserialize(string contents, Connection parent, bool local) {
            var AsXML = XElement.Parse(contents);

            var Name = AsXML.Attribute("Name").Value;
            var ID = int.Parse(AsXML.Attribute("Id").Value);
            var Selection = AsXML.Attribute("Selection").Value;

            return new NetworkEntity(parent, ID, Name, Selection, local);
        }

        /// <summary>
        /// Turn a status change into an XML string
        /// </summary>
        /// <param name="index">The parameter to change</param>
        /// <param name="val">The new value</param>
        /// <returns>An XML string</returns>
        public string SerializeChange(string index, string val) {
            var SWriter = new StringWriter();
            var XWriter = new XmlTextWriter(SWriter);

            var AsXML = new XElement("Change", new XAttribute("Id", _GlobalID),
                new XAttribute("Property", index), new XAttribute("Value", val));

            AsXML.WriteTo(XWriter);
            return SWriter.ToString();
        }

        public static bool PerformChange(string contents, Connection sender) {
            var AsXML = XElement.Parse(contents);

            var Property = AsXML.Attribute("Property").Value;
            var Value = AsXML.Attribute("Value").Value;
            var ID = int.Parse(AsXML.Attribute("Id").Value);

            NetworkEntity target = LKernel.Get<NetworkManager>().Players.Find((e) => e.GlobalID == ID);
            if (target == null) { return false; }
            if (target.owner != sender) { return false; }
            switch (Property) {
                case "Selection":
                    target.Selection = Value;
                    break;
                case "Name":
                    target.Name = Value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public string SerializeLocation() {
            var XKart = new XElement("Kart", new XAttribute("Id", _GlobalID),
                                            new XAttribute("Pos", player.Kart.ActualPosition),
                                            new XAttribute("Vel", player.Kart.VehicleSpeed),
                                            new XAttribute("Or", player.Kart.ActualOrientation));
            return XKart.ToString();
        }


        public static void DeSerializeLocations(string contents)
        {
            var AsXML = XElement.Parse(contents);

            if (true) { }
        }
    }
}
