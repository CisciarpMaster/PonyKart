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
		/// <summary>
		/// This thing's collision group
		/// </summary>
		protected abstract PonykartCollisionGroups CollisionGroup { get; }
		/// <summary>
		/// What does this thing collide with?
		/// </summary>
		protected abstract PonykartCollidesWithGroups CollidesWith { get; }
		/// <summary>
		/// What is the material that this thing has? If you don't override this, you get the default material.
		/// </summary>
		protected virtual PhysicsMaterial PhysicsMaterial { get { return null; } }
		/// <summary>
		/// return 0 for a static body
		/// </summary>
		protected virtual float Mass { get { return 10f; } }

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
			MoreBodyInfoStuff();
			CreateBody();
			SetBodyUserData();
			MoreBodyStuff();
		}

		/// <summary>
		/// Here you create your Actor and assign it.
		/// </summary>
		protected void SetUpBodyInfo() {
			Vector3 inertia;
			CollisionShape.CalculateLocalInertia(Mass, out inertia);
			info = new RigidBodyConstructionInfo(Mass, new MogreMotionState(SpawnPosition, SpawnRotation, Node), CollisionShape, inertia);
			LKernel.Get<PhysicsMaterialManager>().ApplyMaterial(info, PhysicsMaterial ?? LKernel.Get<PhysicsMaterialManager>().DefaultMaterial);
		}

		/// <summary>
		/// Override this if you want to do more to the construction info before it's used to create the body
		/// </summary>
		protected virtual void MoreBodyInfoStuff() { }

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

		/// <summary>
		/// Override this if you want to do more to the rigid body
		/// </summary>
		protected virtual void MoreBodyStuff() { }



		public override void Dispose() {
			if (Body != null) {
				Body.Dispose();
				Body = null;
			}
			base.Dispose();
		}
	}
}
