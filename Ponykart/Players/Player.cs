using System;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Players {
	/// <summary>
	/// Abstract class for players - each player controls a kart, and abstracting away the player will help when it comes to things like AI and/or networking
	/// </summary>
	public abstract class Player {
		/// <summary>
		/// The kart that this player is driving
		/// </summary>
		public Kart Kart { get; protected set; }
		/// <summary>
		/// The driver in the kart
		/// </summary>
		public Driver Driver { get; protected set; }
		/// <summary>
		/// ID number. Same thing that's used as the array index in PlayerManager.
		/// </summary>
		public int ID { get; protected set; }
		/// <summary>
		/// Can the player control his kart?
		/// </summary>
		public virtual bool IsControlEnabled { get; set; }


		public Player(MuffinDefinition def, int id) {
			// don't want to create a player if it's ID isn't valid
			if (id < 0 || id >= Settings.Default.NumberOfPlayers)
				throw new ArgumentOutOfRangeException("id", "ID number specified for kart spawn position is not valid!");
			Launch.Log("[Loading] Player with ID " + id + " created");

			// set up the spawn position/orientation
			Vector3 spawnPos = def.GetVectorProperty("KartSpawnPosition" + id, null);
			Quaternion spawnOrient = def.GetQuatProperty("KartSpawnOrientation" + id, Quaternion.IDENTITY);

			ThingBlock block = new ThingBlock("TwiCutlass", def);
			block.VectorTokens["position"] = spawnPos;
			block.QuatTokens["orientation"] = spawnOrient;

			Kart = LKernel.GetG<Spawner>().Spawn("TwiCutlass", block) as Kart;
			Driver = LKernel.GetG<Spawner>().Spawn("Twilight", block) as Driver;
			Driver.AttachToKart(Kart, Vector3.ZERO);

			Kart.OwnerID = id;
			ID = id;
		}

		/// <summary>
		/// Uses an item
		/// </summary>
		protected abstract void UseItem();


		#region shortcuts
		/// <summary>
		/// Gets the kart's SceneNode
		/// </summary>
		public SceneNode Node { get { return Kart.RootNode; } }
		/// <summary>
		/// Gets the kart's Body
		/// </summary>
		public RigidBody Body { get { return Kart.Body; } }
		/// <summary>
		/// Gets the kart's Node's position. No setter because it's automatically changed to whatever the position of its
		/// body is - use the <see cref="BulletSharp.RigidBody"/> if you want to change the kart's position!
		/// </summary>
		public Vector3 NodePosition {
			get { return Kart.RootNode._getDerivedPosition(); }
		}
		/// <summary>
		/// Gets the kart's SceneNode's orientation
		/// </summary>
		public Quaternion Orientation { get { return Kart.RootNode._getDerivedOrientation(); } }
		#endregion


		public virtual void Detach() {
			Kart = null;
		}
	}
}
