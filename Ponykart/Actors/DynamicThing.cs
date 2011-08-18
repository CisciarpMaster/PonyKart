// draw little yellow circles where the body's origin is? Useful for debugging center-of-mass stuff.
//#define DRAW_BODY_ORIGINS

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
		/// Initial motion state setter. Override this if you want something different. This is only used for initialisation!
		/// </summary>
		protected virtual MotionState DefaultMotionState { get { return new MogreMotionState(SpawnPosition, SpawnRotation, RootNode); } }
		/// <summary>
		/// The actual motion state.
		/// </summary>
		protected MotionState MotionState { get; set; }
		/// <summary>
		/// return 0 for a static body
		/// </summary>
		protected virtual float Mass { get { return 10f; } }

		protected RigidBodyConstructionInfo Info { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicThing(ThingTemplate tt) : base(tt) {
#if DRAW_BODY_ORIGINS
			LKernel.Get<Root>().FrameStarted += FrameStarted;

			var sceneMgr = LKernel.Get<SceneManager>();
			glownode = sceneMgr.RootSceneNode.CreateChildSceneNode("COG" + ID);
			glownode.SetScale(0.2f, 0.2f, 0.2f);
			Entity glowent = sceneMgr.CreateEntity("COG" + ID, "primitives/ellipsoid.mesh");
			glowent.SetMaterialName("FlatGlow_yellow");
			glowent.RenderQueueGroup = Handlers.GlowHandler.RENDER_QUEUE_FLAT_GLOW;
			glowent.CastShadows = false;
			glownode.AttachObject(glowent);
		}

		SceneNode glownode;
		bool FrameStarted(FrameEvent evt) {
			glownode.Position = Body.CenterOfMassPosition;
			return true;
#endif
		}


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
			PreSetUpBodyInfo();
			SetUpBodyInfo();
			PostSetUpBodyInfo();
			CreateBody();
			PostCreateBody();
			SetBodyUserData();
		}

		protected virtual void PreSetUpBodyInfo() { }

		/// <summary>
		/// Here you create your Actor and assign it.
		/// </summary>
		protected void SetUpBodyInfo() {
			Vector3 inertia;
			CollisionShape.CalculateLocalInertia(Mass, out inertia);
			MotionState = DefaultMotionState;
			Info = new RigidBodyConstructionInfo(Mass, MotionState, CollisionShape, inertia);
			LKernel.Get<PhysicsMaterialManager>().ApplyMaterial(Info, PhysicsMaterial ?? LKernel.Get<PhysicsMaterialManager>().DefaultMaterial);
		}

		/// <summary>
		/// Override this if you want to do more to the construction info before it's used to create the body but after it's been created
		/// </summary>
		protected virtual void PostSetUpBodyInfo() { }

		protected void CreateBody() {
			Body = new RigidBody(Info);
			LKernel.Get<PhysicsMain>().World.AddRigidBody(Body, CollisionGroup, CollidesWith);
		}

		/// <summary>
		/// Override this if you want to do more to the rigid body
		/// </summary>
		protected virtual void PostCreateBody() { }

		/// <summary>
		/// Sets the Actor's UserData to this class so we can easily get to it.
		/// </summary>
		protected void SetBodyUserData() {
			Body.UserObject = this;
			Body.SetName(Name);
			Body.SetCollisionGroup(CollisionGroup);
		}


		public override void Dispose() {
#if DRAW_BODY_ORIGINS
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
#endif
			if (Body != null) {
				Body.Dispose();
				Body = null;
			}
			base.Dispose();
		}
	}
}
