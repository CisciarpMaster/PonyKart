using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Handlers;
using Ponykart.Levels;

namespace Ponykart.Physics {

	public delegate void TriggerReportHandler(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags);

	public class TriggerRegion : System.IDisposable {
		public GhostObject Ghost { get; protected set; }
		public string Name { get; protected set; }
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public HashSet<RigidBody> CurrentlyCollidingWith { get; set; }

		/// <summary>
		/// Invoked by TriggerReporter
		/// </summary>
		public event TriggerReportHandler OnTrigger;

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		public TriggerRegion(string name, Vector3 position, CollisionShape shape) : this(name, position, Vector3.ZERO, shape) { }

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		/// <param name="rotation">a degree vector</param>
		public TriggerRegion(string name, Vector3 position, Vector3 rotation, CollisionShape shape) {
			Name = name;
			CurrentlyCollidingWith = new HashSet<RigidBody>();
			
			// mogre
			var sceneMgr = LKernel.Get<SceneManager>();

			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(name);
			// make a mesh for the region depending on what its type is
			switch (shape.ShapeType) {
				case BroadphaseNativeType.BoxShape: 
					Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
					Node.SetScale((shape as BoxShape).HalfExtentsWithoutMargin * 2);
					break;
				case BroadphaseNativeType.CapsuleShape:
					Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
					Vector3 vec = new Vector3();
					vec.y = (shape as CapsuleShape).HalfHeight * 2;
					vec.x = vec.z = (shape as CapsuleShape).Radius;
					Node.SetScale(vec);
					break;
				case BroadphaseNativeType.CylinderShape:
					Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
					Vector3 vec2 = new Vector3();
					vec2.y = (shape as CylinderShape).HalfExtentsWithoutMargin.y;
					vec2.x = vec2.z = (shape as CylinderShape).Radius;
					Node.SetScale(vec2);
					break;
				case BroadphaseNativeType.SphereShape:
					Entity = sceneMgr.CreateEntity(name, "primitives/ellipsoid.mesh");
					float dim = (shape as SphereShape).Radius;
					Node.SetScale(dim, dim, dim);
					break;
				default:
					// for things like meshes, convex hulls, etc
					Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
					break;
			}
			SetBalloonGlowColor(BalloonGlowColor.orange);
			Entity.RenderQueueGroup = GlowHandler.RENDER_QUEUE_BUBBLE_GLOW;
			Entity.CastShadows = false;

			Node.AttachObject(Entity);
			Node.Position = position;
			Node.Rotate(rotation.DegreeVectorToGlobalQuaternion());

			// physics
			Matrix4 transform = new Matrix4(rotation.DegreeVectorToGlobalQuaternion());
			transform.SetTrans(position);

			// make our ghost object
			Ghost = new GhostObject();
			Ghost.CollisionFlags = CollisionFlags.NoContactResponse;
			Ghost.CollisionShape = shape;
			Ghost.WorldTransform = transform;
			Ghost.SetName(name);
			LKernel.Get<PhysicsMain>().World.AddCollisionObject(Ghost, PonykartCollisionGroups.Triggers, PonykartCollidesWithGroups.Triggers);

			// then add this to the trigger reporter
			LKernel.Get<TriggerReporter>().Regions.Add(name, this);
		}

		/// <summary>
		/// Run the enter event
		/// </summary>
		public void InvokeTrigger(RigidBody otherBody, TriggerReportFlags flags) {
			// at the moment this only triggers when the "main" shape of an actor enters. Do we want to change this?
			if (OnTrigger != null)
				OnTrigger(this, otherBody, flags);
		}

		/// <summary>
		/// Must be one of: red, blue, yellow, green, orange, magenta, purple, cyan, white
		/// </summary>
		public void SetBalloonGlowColor(BalloonGlowColor color) {
			Entity.SetMaterialName("BalloonGlow_" + color);
		}

		/// <summary>
		/// do we need to dispose of this stuff? don't we nuke the scene manager every time?
		/// </summary>
		public void Dispose() {
			var sceneMgr = LKernel.Get<SceneManager>();

			if (Node != null) {
				if (LKernel.Get<LevelManager>().IsValidLevel) {
					sceneMgr.DestroyEntity(Entity);
					sceneMgr.DestroySceneNode(Node);
				}
				Entity.Dispose();
				Node.Dispose();
				Entity = null;
				Node = null;
			}
		}
	}
}
