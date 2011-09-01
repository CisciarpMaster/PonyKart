using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Handlers;
using Ponykart.Levels;
using Ponykart.Properties;

namespace Ponykart.Physics {

	public delegate void TriggerReportHandler(TriggerRegion region, RigidBody otherBody, TriggerReportFlags flags);

	public class TriggerRegion : System.IDisposable {
		public RigidBody Body { get; protected set; }
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
		public TriggerRegion(string name, Vector3 position, CollisionShape shape) : this(name, position, Quaternion.IDENTITY, shape) { }

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		/// <param name="rotation">a degree vector</param>
		public TriggerRegion(string name, Vector3 position, Quaternion rotation, CollisionShape shape) {
			Name = name;
			CurrentlyCollidingWith = new HashSet<RigidBody>();
			
			// mogre
			var sceneMgr = LKernel.GetG<SceneManager>();

			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(name);
			if (Settings.Default.EnableGlowyRegions) {
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
						vec.x = vec.z = (shape as CapsuleShape).Radius * 2;
						Node.SetScale(vec);
						break;
					case BroadphaseNativeType.CylinderShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
						Vector3 vec2 = new Vector3();
						vec2.y = (shape as CylinderShape).HalfExtentsWithoutMargin.y;
						vec2.x = vec2.z = (shape as CylinderShape).Radius * 2;
						Node.SetScale(vec2);
						break;
					case BroadphaseNativeType.SphereShape:
						Entity = sceneMgr.CreateEntity(name, "primitives/sphere.mesh");
						float dim = (shape as SphereShape).Radius * 2;
						Node.SetScale(dim, dim, dim);
						break;
					default:
						// for things like meshes, convex hulls, etc
						Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
						break;
				}
				GlowColor = BalloonGlowColor.red;
				Entity.RenderQueueGroup = GlowHandler.RENDER_QUEUE_BUBBLE_GLOW;
				Entity.CastShadows = false;

				Node.AttachObject(Entity);
			}
			Node.Position = position;
			Node.Orientation = rotation;

			// physics
			Matrix4 transform = new Matrix4(rotation);
			transform.SetTrans(position);

			// make our ghost object
			Body = new RigidBody(new RigidBodyConstructionInfo(0, new MogreMotionState(Node), shape));
			Body.CollisionFlags = CollisionFlags.NoContactResponse;
			Body.CollisionShape = shape;
			Body.WorldTransform = transform;
			Body.SetName(name);
			LKernel.GetG<PhysicsMain>().World.AddCollisionObject(Body, PonykartCollisionGroups.Triggers, PonykartCollidesWithGroups.Triggers);

			// then add this to the trigger reporter
			LKernel.GetG<TriggerReporter>().Regions.Add(name, this);
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
		public BalloonGlowColor GlowColor {
			get {
				return balloonColor;
			}
			set {
				balloonColor = value;
				if (Settings.Default.EnableGlowyRegions)
					Entity.SetMaterialName("BalloonGlow_" + value);
			}
		}
		BalloonGlowColor balloonColor = BalloonGlowColor.red;

		/// <summary>
		/// do we need to dispose of this stuff? don't we nuke the scene manager every time?
		/// </summary>
		public void Dispose() {
			var sceneMgr = LKernel.GetG<SceneManager>();

			if (Node != null) {
				if (LKernel.GetG<LevelManager>().IsValidLevel) {
					if (Settings.Default.EnableGlowyRegions)
						sceneMgr.DestroyEntity(Entity);
					sceneMgr.DestroySceneNode(Node);
				}
				if (Settings.Default.EnableGlowyRegions) {
					Entity.Dispose();
					Entity = null;
				}
				Node.Dispose();
				Node = null;
			}
		}
	}
}
