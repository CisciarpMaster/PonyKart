using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Stuff;

namespace Ponykart.Players {
	/// <summary>
	/// Abstract class for players - each player controls a kart, and abstracting away the player will help when it comes to things like AI and/or networking
	/// </summary>
	public abstract class Player : IDisposable {
		/// <summary>
		/// The kart that this player is driving
		/// </summary>
		public Kart Kart { get; protected set; }
		/// <summary>
		/// ID number. Same thing that's used as the array index in PlayerManager.
		/// </summary>
		protected int ID;


		public Player(int id) {
			// don't want to create a player if it's ID isn't valid
			if (id < 0 || id >= Constants.NUMBER_OF_PLAYERS)
				throw new ArgumentOutOfRangeException("id", "ID number specified for kart spawn position is not valid!");
			Launch.Log("[Loading] Player with ID " + id + " created");

			Kart = LKernel.Get<Spawner>().Spawn(ThingEnum.Kart, "Kart", LKernel.Get<KartSpawnPositions>().GetPosition(id)) as Kart;
			ID = id;
		}

		#region Stuff to override
		/// <summary>
		/// Uses an item
		/// </summary>
		protected abstract void UseItem();
		#endregion


		#region shortcuts
		/// <summary>
		/// Gets the kart's SceneNode
		/// </summary>
		public SceneNode Node { get { return Kart.Node; } }
		/// <summary>
		/// Gets the kart's Actor
		/// </summary>
		public Actor Actor { get { return Kart.Actor; } }
		/// <summary>
		/// Gets/sets the kart's Actor's position
		/// </summary>
		public Vector3 ActorPosition {
			get { return Kart.Actor.GlobalPosition; }
			set { Kart.Actor.GlobalPosition = value; }
		}
		/// <summary>
		/// Gets the kart's Node's position. No setter because it's automatically changed to whatever the position of its
		/// actor - use <see cref="ActorPosition"/> if you want to change the kart's position!
		/// </summary>
		public Vector3 NodePosition {
			get { return Kart.Node.Position; }
		}
		/// <summary>
		/// Gets the kart's SceneNode's orientation
		/// </summary>
		public Quaternion Orientation { get { return Kart.Node.Orientation; } }
		#endregion


		public virtual void Dispose() {
			Kart = null;
		}
	}
}
