using System.IO;
using BulletSharp;
using Mogre;
using Ponykart.Physics;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents a physics collision shape
	/// </summary>
	public class ShapeComponent : LDisposable {
		public CollisionShape Shape { get; protected set; }
		public Matrix4 Transform { get; protected set; }

		/// <summary>
		/// For physics
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public ShapeComponent(LThing lthing, ThingBlock template, ShapeBlock block) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Shape = block.Shape;
			Transform = block.Transform;

			// if our shape is a hull, this loads the hull mesh
			if (block.EnumTokens["type"] == ThingEnum.Hull) {
				
				string name = block.GetStringProperty("hullname", string.Empty);

				if (!string.IsNullOrEmpty(name) && File.Exists(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension)) {
					Shape = LKernel.GetG<PhysicsMain>().ImportCollisionShape(name);
				}
				else {
					string meshName = block.GetStringProperty("mesh", null);

					// TODO: need a better way of loading a mesh
					Entity ent = LKernel.GetG<SceneManager>().CreateEntity(meshName);

					TriangleMesh trimesh = OgreToBulletMesh.Convert(ent.GetMesh(), Transform.GetTrans(), Transform.ExtractQuaternion(), Vector3.UNIT_SCALE);
					Shape = new ConvexTriangleMeshShape(trimesh);

					LKernel.GetG<SceneManager>().DestroyEntity(ent);
					ent.Dispose();

					// TODO: figure out how to deal with convex triangle mesh shapes since apparently they aren't being exported
					//LKernel.GetG<PhysicsMain>().SerializeShape(Shape, name);
				}
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (!Shape.IsDisposed)
				Shape.Dispose();

			base.Dispose(disposing);
		}
	}
}