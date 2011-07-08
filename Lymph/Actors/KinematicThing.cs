using System.Collections.ObjectModel;
using Lymph.Phys;
using Mogre;
using Mogre.PhysX;

namespace Lymph.Actors {
	/// <summary>
	/// Kinematic things are another one of those weird things physx offers that doesn't actually exist in real life.
	/// Basically a kinematic actor is one that can exert force on others, but any force exerted on it does nothing.
	/// So it's best for things that you want to move around by themselves but aren't able to be pushed around.
	/// This works well for things like NPCs, decorations, etc.
	/// 
	/// You also can't apply any force to kinematic actors. You have to move them around the hard way.
	/// 
	/// When moving these around, make sure you use the Actor.MoveGlobal___() methods as they're made just for kinematic actors!
	/// 
	/// Kinematic actors aren't affected by dynamic actors, or by other kinematic actors (this includes the level!). Or hell, even gravity!
	/// Kinematic actors are basically for non-moving NPCs and things like moving platforms. If you want an NPC that chases you, a kinematic
	/// actor could work but then you'd have to do all the collision detection yourself.
	/// 
	/// Controllers collide with kinematic actors.
	/// 
	/// More info here: http://www.gamedev.net/topic/467249-physx---am-i-understanding-this-right/
	/// </summary>
	public abstract class KinematicThing : Thing {
		/// <summary>
		/// The physx body that the Node is attached to
		/// </summary>
		public Actor Actor { get; private set; }
		/// <summary>
		/// ShapeDesc for the "main" body. This should be slightly larger than that of the Actor's.
		/// </summary>
		protected abstract ShapeDesc ShapeDesc { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public KinematicThing(ThingTemplate tt) : base(tt) {}


		#region Physics
		/// <summary>
		/// This method does the following:
		/// - Creates an Actor
		/// - Assigns a collision group ID
		/// - Attaches it to a SceneNode
		/// - Sets some properties
		/// It does all of this through the appropriate various methods.
		/// 
		/// This is called automatically from the constructor.
		/// </summary>
		protected override void SetUpPhysics() {
			CreateActor();
			AssignCollisionGroupIDToShapes();
			AttachToSceneNode();
			SetBodyUserData();
			SetDefaultActorProperties();
		}

		/// <summary>
		/// Here you create your Actor and assign it.
		/// </summary>
		protected void CreateActor() {
			BodyDesc bd = new BodyDesc();
			bd.BodyFlags.Kinematic = true;
			ActorDesc ad = new ActorDesc(bd, 1, ShapeDesc);
			Actor = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			Actor.Name = Node.Name;
		}

		/// <summary>
		/// Assigns the collision group ID defined in CollisionGroupID to the shapes of the thing.
		/// </summary>
		protected void AssignCollisionGroupIDToShapes() {
			ReadOnlyCollection<Shape> shapes = Actor.Shapes;
			foreach (Shape s in shapes)
				s.Group = CollisionGroupID;
			Actor.Group = CollisionGroupID;
		}

		/// <summary>
		/// Attaches the Actor to the Node and sets the position and orientation of the Actor to match that of the Node.
		/// </summary>
		protected void AttachToSceneNode() {
			if (Actor != null && Node != null) {
				Actor.GlobalPosition = Node.Position;
				Actor.GlobalOrientationQuaternion = Node.Orientation;
			}
		}

		/// <summary>
		/// Sets the Actor's UserData to this class so we can easily get to it.
		/// </summary>
		protected void SetBodyUserData() {
			if (Actor != null)
				Actor.UserData = this;
		}

		/// <summary>
		/// Sets default thing properties.
		/// - Aligns it to the XZ plane
		/// - Sets linear and angular damping
		/// - activates the sleep thingy
		/// </summary>
		protected void SetDefaultActorProperties() {
			Actor.BodyFlags.FrozenPosY = true;
			Actor.LinearDamping = 0.1f;
			Actor.AngularDamping = 0.1f;
			Actor.BodyFlags.EnergySleepTest = true;
		}
		#endregion Physics

		public override void Dispose() {
			if (Actor != null) {
				Actor.Dispose();
				Actor = null;
			}
			base.Dispose();
		}
	}
}
