using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Handlers;
using Ponykart.Levels;

namespace Ponykart.Phys {
	public class TriggerRegion : IDisposable {
		public Actor Actor { get; protected set; }
		public Shape Shape { get; protected set; }
		public string Name { get; protected set; }
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }

		public event TriggerReportHandler OnTriggerEnter;
		public event TriggerReportHandler OnTriggerLeave;

		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <param name="position"></param>
		/// <param name="rotation">a degree vector</param>
		/// <param name="desc"></param>
		public TriggerRegion(string name, Vector3 position, Vector3 rotation, ShapeDesc desc) {
			Name = name;

			// physics
			desc.ShapeFlags |= ShapeFlags.TriggerEnable | ShapeFlags.DisableCollision | ShapeFlags.DisableRaycasting;

			ActorDesc ad = new ActorDesc(name, desc);
			ad.GlobalPosition = position;
			ad.GlobalOrientation = new Quaternion().FromLocalEuler(rotation.DegreeVectorToRadianVector()).ToRotationMatrix();

			Actor = LKernel.Get<PhysXMain>().Scene.CreateActor(ad);
			Shape = Actor.Shapes[0];
			
			// mogre
			var sceneMgr = LKernel.Get<SceneManager>();


			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(name);
			switch (desc.Type) {
				case ShapeTypes.Box: 
					Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
					Node.SetScale((desc as BoxShapeDesc).Dimensions);
					break;
				case ShapeTypes.Capsule:
					Entity = sceneMgr.CreateEntity(name, "primitives/cylinder.mesh");
					Vector3 vec = new Vector3();
					vec.y = (desc as CapsuleShapeDesc).Height;
					vec.x = vec.z = (desc as CapsuleShapeDesc).Radius * 2;
					Node.SetScale(vec);
					break;
				case ShapeTypes.Sphere:
					Entity = sceneMgr.CreateEntity(name, "primitives/ellipsoid.mesh");
					float dim = (desc as SphereShapeDesc).Radius * 2;
					Node.SetScale(dim, dim, dim);
					break;
				default:
					Entity = sceneMgr.CreateEntity(name, "primitives/box.mesh");
					break;
			}
			Entity.SetMaterialName("BalloonGlow_orange");
			Entity.RenderQueueGroup = GlowHandler.RENDER_QUEUE_BUBBLE_GLOW;

			Node.AttachObject(Entity);
			Node.Position = Actor.GlobalPosition;
			Node.Orientation.FromLocalEuler(rotation);
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
