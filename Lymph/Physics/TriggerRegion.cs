using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Handlers;
using Ponykart.Levels;

namespace Ponykart.Phys {

	public delegate void TriggerReportHandler(TriggerRegion region, Shape otherShape, TriggerFlags flags);

	public class TriggerRegion : IDisposable {
		public Actor Actor { get; protected set; }
		public Shape Shape { get; protected set; }
		public string Name { get; protected set; }
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }

		/// <summary>
		/// Invoked by TriggerReporter
		/// </summary>
		public event TriggerReportHandler OnTrigger;

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		public TriggerRegion(string name, Vector3 position, ShapeDesc desc) : this(name, position, Vector3.ZERO, desc) { }

		/// <summary>
		/// Creates a new trigger region. It automatically adds itself to the TriggerReporter's dictionary, so you don't have to do that.
		/// </summary>
		/// <param name="rotation">a degree vector</param>
		public TriggerRegion(string name, Vector3 position, Vector3 rotation, ShapeDesc desc) {
			Name = name;

			// physics
			desc.ShapeFlags |= ShapeFlags.TriggerEnable | ShapeFlags.DisableRaycasting;

			ActorDesc ad = new ActorDesc(name, desc);
			ad.GlobalPosition = position;
			ad.GlobalOrientation = rotation.DegreeVectorToGlobalQuaternion().ToRotationMatrix();

			Actor = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			Shape = Actor.Shapes[0];
			Shape.Name = name; // without this line, the reporter doesn't know how to find our trigger region object
			
			// mogre
			var sceneMgr = LKernel.Get<SceneManager>();

			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(name);
			switch (desc.Type) {
				case ShapeTypes.Box: 
					Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
					Node.SetScale((desc as BoxShapeDesc).Dimensions * 2);
					break;
				case ShapeTypes.Capsule:
					Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
					Vector3 vec = new Vector3();
					vec.y = (desc as CapsuleShapeDesc).Height;
					vec.x = vec.z = (desc as CapsuleShapeDesc).Radius;
					Node.SetScale(vec);
					break;
				case ShapeTypes.Sphere:
					Entity = sceneMgr.CreateEntity(name, "primitives/ellipsoid.mesh");
					float dim = (desc as SphereShapeDesc).Radius;
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
			Node.Position = Actor.GlobalPosition;
			Node.Rotate(rotation.DegreeVectorToGlobalQuaternion());

			// then add this to the trigger reporter
			LKernel.Get<TriggerReporter>().Regions.Add(name, this);
		}

		/// <summary>
		/// Run the enter event
		/// </summary>
		public void InvokeTrigger(Shape otherShape, TriggerFlags flags) {
			// at the moment this only triggers when the "main" shape of an actor enters. Do we want to change this?
			if (otherShape == otherShape.Actor.Shapes[0]) {

				if (OnTrigger != null)
					OnTrigger(this, otherShape, flags);
			}
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
