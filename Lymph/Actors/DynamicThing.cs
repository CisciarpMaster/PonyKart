using System.Collections.ObjectModel;
using Lymph.Phys;
using Mogre;
using Mogre.PhysX;

namespace Lymph.Actors {
	/// <summary>
	/// Dynamic things are basically what you have in real life. If something hits something else, both of them will be affected.
	/// You don't really want dynamic things for NPCs and so on, but they work well for movable scenery.
	/// </summary>
	public abstract class DynamicThing : Thing {
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
		public DynamicThing(ThingTemplate tt) : base(tt) {}


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
			ActorDesc ad = new ActorDesc(new BodyDesc(), 1, ShapeDesc);
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
