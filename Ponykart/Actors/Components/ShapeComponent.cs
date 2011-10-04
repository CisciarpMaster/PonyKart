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
		// if your shape is imported from a .bullet file, then when the BulletWorldImporter destroys everything we don't want to try to
		// dispose our shape, otherwise we get an exception
		private bool IsShapeImportedFromBulletFile = false;

		/// <summary>
		/// For physics
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="block">The block we're creating this component from</param>
		public ShapeComponent(LThing lthing, ShapeBlock block) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Shape = block.Shape;
			Transform = block.Transform;

			// if our shape is a hull, this loads the hull mesh
			if (block.EnumTokens["type"] == ThingEnum.Hull) {
				
				string name = block.GetStringProperty("hullname", string.Empty);

				if (!string.IsNullOrEmpty(name) && File.Exists(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension)) {
					Shape = LKernel.GetG<PhysicsMain>().ImportCollisionShape(name);
					IsShapeImportedFromBulletFile = true;
				}
				else {
					string meshName = block.GetStringProperty("mesh", null);

					// TODO: need a better way of loading a mesh
					Entity ent = LKernel.GetG<SceneManager>().CreateEntity(meshName);

					TriangleMesh trimesh = OgreToBulletMesh.Convert(ent.GetMesh(), Transform.GetTrans(), Transform.ExtractQuaternion(), Vector3.UNIT_SCALE);
					Shape = new ConvexTriangleMeshShape(trimesh);

					LKernel.GetG<SceneManager>().DestroyEntity(ent);
					ent.Dispose();

					// TODO: figure out how to deal with concave triangle mesh shapes since apparently they aren't being exported
					//LKernel.GetG<PhysicsMain>().SerializeShape(Shape, name);
				}
			}
			// for a trimesh
			else if (block.EnumTokens["type"] == ThingEnum.Mesh) {
				// example.mesh
				string meshName = block.GetStringProperty("mesh", null);
				// example
				// physics/example.bullet
				string name, bulletFilePath;

				if (meshName.EndsWith(".mesh")) {
					name = meshName.Remove(meshName.IndexOf(".mesh"));
					bulletFilePath = Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension;
				}
				else if (meshName.EndsWith(".bullet")) {
					name = meshName.Remove(meshName.IndexOf(".bullet"));
					bulletFilePath = Settings.Default.BulletFileLocation + meshName;
				}
				else {
					throw new System.ApplicationException("Your \"Mesh\" property needs to end in either .mesh or .bullet!");
				}

				// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
				if (File.Exists(bulletFilePath)) {
					// so it has a file
					Shape = LKernel.GetG<PhysicsMain>().ImportCollisionShape(name);
					IsShapeImportedFromBulletFile = true;
				}
				else {
					Launch.Log("[ShapeComponent] " + bulletFilePath + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");

					// it does not have a file, so we need to convert our ogre mesh
					Entity ent = LKernel.GetG<SceneManager>().CreateEntity(meshName);

					Shape = new BvhTriangleMeshShape(
						OgreToBulletMesh.Convert(
							ent.GetMesh(),
							block.GetVectorProperty("Position", null),
							block.GetQuatProperty("Orientation", Quaternion.IDENTITY),
							block.GetVectorProperty("Scale", Vector3.UNIT_SCALE)),
						true, true);

					// and then export it as a .bullet file
					LKernel.GetG<PhysicsMain>().SerializeShape(Shape, name);

					LKernel.GetG<SceneManager>().DestroyEntity(ent);
					ent.Dispose();
				}
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (!Shape.IsDisposed && !IsShapeImportedFromBulletFile)
				Shape.Dispose();

			base.Dispose(disposing);
		}
	}
}