using Ponykart.Actors;

namespace Ponykart.Physics {
	/// <summary>
	/// A little helper object for CollisionObjects that don't have an LThing associated with them. This holds some data for them instead.
	/// </summary>
	public class CollisionObjectDataHolder {
		/// <summary>
		/// The object's collision group
		/// </summary>
		public PonykartCollisionGroups CollisionGroup { get; private set; }
		/// <summary>
		/// The object's name
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Do we care about collision events or not?
		/// </summary>
		public bool CareAboutCollisionEvents { get; private set; }
		/// <summary>
		/// If this has a LThing associated with it, this points to it.
		/// </summary>
		public LThing Thing { get; private set; }


		/// <summary>
		/// Create a data holder without a LThing object.
		/// </summary>
		/// <param name="collisionGroup">The collision group of the object</param>
		/// <param name="name">The name of the object, excluding an ID</param>
		/// <param name="careAboutCollisionEvents">Do we care about collision events?</param>
		public CollisionObjectDataHolder(PonykartCollisionGroups collisionGroup, string name, bool careAboutCollisionEvents) {
			this.CollisionGroup = collisionGroup;
			this.Name = name;
			this.CareAboutCollisionEvents = careAboutCollisionEvents;
		}

		/// <summary>
		/// Create a data holder using properties of an LThing object.
		/// </summary>
		public CollisionObjectDataHolder(LThing thing)
			: this(thing.CollisionGroup, thing.Name, thing.CareAboutCollisionEvents)
		{
			this.Thing = thing;
		}
	}
}
