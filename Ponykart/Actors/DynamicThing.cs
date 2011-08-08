using BulletSharp;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Actors {
	/// <summary>
	/// Dynamic things are basically what you have in real life. If something hits something else, both of them will be affected.
	/// You don't really want dynamic things for NPCs and so on, but they work well for movable scenery.
	/// </summary>
	public abstract class DynamicThing : Thing {
		/// <summary>
		/// The bullet body that the Node is attached to
		/// </summary>
		public RigidBody Body { get; protected set; }
		/// <summary>
		/// TODO
		/// From the wiki:
		/// 
		/// Each rigid body needs to reference a collision shape. The collision shape is for collisions only, and thus has no concept
		/// of mass, inertia, restitution, etc. If you have many bodies that use the same collision shape (eg every spaceship in your
		/// simulation is a 5-unit-radius sphere), it is good practice to have only one Bullet collision shape, and share it among all
		/// those bodies.
		/// </summary>
		protected abstract CollisionShape CollisionShape { get; }

		protected abstract PonykartCollisionGroups CollisionGroup { get; }
		protected abstract PonykartCollidesWithGroups CollidesWith { get; }

		/// <summary>
		/// return 0 for a static body
		/// </summary>
		protected virtual float Mass { get { return 1f; } }

		protected RigidBodyConstructionInfo info;

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicThing(ThingTemplate tt) : base(tt) { }


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
			SetUpBodyInfo();
			SetDefaultActorProperties();
			CreateBody();
			SetBodyUserData();
		}

		/// <summary>
		/// Here you create your Actor and assign it.
		/// </summary>
		protected virtual void SetUpBodyInfo() {
			Vector3 inertia;
			CollisionShape.CalculateLocalInertia(Mass, out inertia);
			info = new RigidBodyConstructionInfo(Mass, new MogreMotionState(SpawnPosition, SpawnRotation, Node), CollisionShape, inertia);
		}

		/// <summary>
		/// Sets default info properties, like linear and angular damping.
		/// Remember that we can't edit the Body itself! We need to edit the info object!
		/// </summary>
		protected virtual void SetDefaultActorProperties() {
			info.LinearDamping = 0.1f;
			info.AngularDamping = 0.1f;
		}

		protected void CreateBody() {
			Body = new RigidBody(info);
			LKernel.Get<PhysicsMain>().World.AddRigidBody(Body, CollisionGroup.ToBullet(), CollidesWith.ToBullet());
		}

		/// <summary>
		/// Sets the Actor's UserData to this class so we can easily get to it.
		/// </summary>
		protected void SetBodyUserData() {
			Body.UserObject = this;
			Body.SetName(Name);
			Body.SetCollisionGroup(CollisionGroup);
		}



		public override void Dispose() {
			if (Body != null) {
				Body.Dispose();
				Body = null;
			}
			base.Dispose();
		}
	}
}
