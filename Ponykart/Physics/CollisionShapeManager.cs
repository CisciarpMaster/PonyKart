using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BulletSharp;
using BulletSharp.Serialize;
using Mogre;
using Ponykart.Actors;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Physics {
	public class CollisionShapeManager {
		IDictionary<string, CollisionShape> Shapes;

		public CollisionShapeManager() {
			Shapes = new Dictionary<string, CollisionShape>();
		}

		/// <summary>
		/// Gets a collision shape.
		/// </summary>
		/// <param name="name">The name of the shape to get</param>
		public CollisionShape GetShape(string name) {
			return Shapes[name];
		}

		/// <summary>
		/// Gets a collision shape from a .bullet file.
		/// </summary>
		/// <param name="filename">The full filename, <u>including extension</u></param>
		/// <param name="ent">If the .bullet file does not exist, this is the entity we should use to generate it</param>
		/// <param name="node">If the .bullet file does not exist, this is the node we will use to get info from when generating it</param>
		/// <returns></returns>
		public CollisionShape GetShapeFromFile(string filename, Entity ent, SceneNode node) {
			CollisionShape shape;

			if (!Shapes.TryGetValue(filename, out shape)) {
				// check to see if the .bullet file exists
				if (File.Exists(Settings.Default.BulletFileLocation + filename)) {
					// if it does not, import it (make sure we get rid of the extension first)
					shape = ImportCollisionShape(Path.GetFileNameWithoutExtension(filename));
				}
				else {
					Launch.Log("[PhysicsMain] " + filename + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");
					// it does not have a file, so we need to convert our ogre mesh
					shape = new BvhTriangleMeshShape(OgreToBulletMesh.Convert(ent, node), true, true);
					(shape as BvhTriangleMeshShape).BuildOptimizedBvh();
					// and then export it as a .bullet file
					SerializeShape(shape, node.Name);
				}

				// add the shape to the dictionary, including the .bullet extension
				Shapes.Add(filename, shape);
			}

			return shape;
		}

		/// <summary>
		/// Creates a CollisionShape from the ShapeComponents of the given thing. If the shape already exists, we'll just return that instead.
		/// </summary>
		public CollisionShape CreateAndRegisterShape(LThing thing, ThingDefinition def) {
			CollisionShape shape;
			if (!Shapes.TryGetValue(thing.Name, out shape)) {
				// create the shape

				bool forceCompound = def.GetBoolProperty("forcecompound", false);

				// if we only have one shape we don't have to do as much
				if (thing.ShapeComponents.Count == 1) {
					// force us to use a compound shape?
					if (forceCompound) {
						CompoundShape comp = new CompoundShape();
						comp.AddChildShape(thing.ShapeComponents[0].Transform, CreateShapeForComponent(thing.ShapeComponents[0]));

						shape = comp;
					}
					// one component, no compound is the easiest
					else {
						shape = CreateShapeForComponent(thing.ShapeComponents[0]);
					}
				}
				// otherwise, make all of our shapes and stick them in a compound shape
				else {
					CompoundShape comp = new CompoundShape();

					foreach (ShapeComponent component in thing.ShapeComponents) {
						comp.AddChildShape(component.Transform, CreateShapeForComponent(component));
					}

					shape = comp;
				}

				// then put the shape in our dictionary
				Shapes.Add(thing.Name, shape);
			}
			return shape;
		}

		/// <summary>
		/// If you've already created a shape, this registers it
		/// </summary>
		public void RegisterShape(string name, CollisionShape shape) {
			Shapes.Add(name, shape);
		}

		/// <summary>
		/// Tries to get a shape from the shape dictionary. Just a wrapper around the dictionary's own TryGetValue function, so it works in the same way.
		/// </summary>
		/// <param name="name">The name of the shape you're trying to get</param>
		/// <param name="shape">Returns the shape, if successful</param>
		/// <returns>Returns true if successful, false if not.</returns>
		public bool TryGetShape(string name, out CollisionShape shape) {
			return Shapes.TryGetValue(name, out shape);
		}

		/// <summary>
		/// Creates a collision shape for a shape component
		/// </summary>
		private CollisionShape CreateShapeForComponent(ShapeComponent component) {
			switch (component.Type) {
				case ThingEnum.Box:
					return new BoxShape(component.Dimensions);
				case ThingEnum.Cylinder:
					return new CylinderShape(component.Dimensions);
				case ThingEnum.Cone:
					var cone = new ConeShape(component.Radius, component.Height);
					cone.ConeUpIndex = 1;
					return cone;
				case ThingEnum.Capsule:
					return new CapsuleShape(component.Radius, component.Height);
				case ThingEnum.Sphere:
					return new SphereShape(component.Radius);
				case ThingEnum.Hull: {
						CollisionShape shape;
						// physics/example.bullet
						string name = Path.GetFileNameWithoutExtension(component.Mesh);
						string bulletFilePath;

						if (component.Mesh.EndsWith(".mesh")) {
							bulletFilePath = Settings.Default.BulletFileLocation + name + ".bullet";
						}
						else if (component.Mesh.EndsWith(".bullet")) {
							bulletFilePath = Settings.Default.BulletFileLocation + component.Mesh;
						}
						else {
							throw new ApplicationException("Your \"Mesh\" property needs to end in either .mesh or .bullet!");
						}

						// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
						if (File.Exists(bulletFilePath)) {
							// so it has a file
							shape = ImportCollisionShape(name);
						}
						else {
							var sceneMgr = LKernel.GetG<SceneManager>();
							// get our entity if we have one, create it if we don't
							Entity ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

							ConvexHullShape hull = OgreToBulletMesh.ConvertToHull(
								ent.GetMesh(),
								component.Transform.GetTrans(),
								component.Transform.ExtractQuaternion(),
								Vector3.UNIT_SCALE);
							shape = hull;

							// TODO: figure out how to deal with concave triangle mesh shapes since apparently they aren't being exported
							SerializeShape(shape, name);
						}
						return shape;
					}
				case ThingEnum.Mesh: {
						CollisionShape shape;
						// example
						// physics/example.bullet
						string name = Path.GetFileNameWithoutExtension(component.Mesh);
						string bulletFilePath;

						if (component.Mesh.EndsWith(".mesh")) {
							bulletFilePath = Settings.Default.BulletFileLocation + name + ".bullet";
						}
						else if (component.Mesh.EndsWith(".bullet")) {
							bulletFilePath = Settings.Default.BulletFileLocation + component.Mesh;
						}
						else {
							throw new ApplicationException("Your \"Mesh\" property needs to end in either .mesh or .bullet!");
						}

						// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
						if (File.Exists(bulletFilePath)) {
							// so it has a file
							shape = ImportCollisionShape(name);
						}
						else {
							Launch.Log("[CollisionShapeManager] " + bulletFilePath + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");

							// it does not have a file, so we need to convert our ogre mesh
							var sceneMgr = LKernel.GetG<SceneManager>();
							Entity ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

							shape = new BvhTriangleMeshShape(
								OgreToBulletMesh.Convert(
									ent.GetMesh(),
									component.Transform.GetTrans(),
									component.Transform.ExtractQuaternion(),
									Vector3.UNIT_SCALE),
								true,
								true);

							(shape as BvhTriangleMeshShape).BuildOptimizedBvh();

							// and then export it as a .bullet file
							SerializeShape(shape, name);
						}
						return shape;
					}
				case ThingEnum.Heightmap: {
						string filename = Settings.Default.BulletFileLocation + component.Mesh;
						//FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
						Bitmap bitmap = new Bitmap(filename);

						int width = 256;
						int length = 256;

						byte[] terr = new byte[width * length * 4];
						MemoryStream file = new MemoryStream(terr);
						BinaryWriter writer = new BinaryWriter(file);
						for (int i = 0; i < width; i++) {
							for (int j = 0; j < length; j++) {
								writer.Write(bitmap.GetPixel((int) (((float) i / width) * bitmap.Width), (int) (((float) j / length) * bitmap.Height)).R / 255f);
								//writer.Write(bitmap.GetPixel(i, j).R / 255f);
							}
						}
						writer.Flush();
						file.Position = 0;

						float heightScale = component.MaxHeight - component.MinHeight / 255f;
						Vector3 scale = component.Dimensions;

						var heightfield = new HeightfieldTerrainShape(width, length, file, heightScale,
							component.MinHeight, component.MaxHeight, 1, PhyScalarType.PhyFloat, false);

						//heightfield.SetUseDiamondSubdivision(true);
						//heightfield.LocalScaling = new Vector3(scale.x / width, scale.y, scale.z / length);

						//Matrix4 trans = new Matrix4();
						//trans.MakeTransform(new Vector3(-scale.x / 2f, scale.y / 2f, -scale.z / 2f), new Vector3(scale.x, 1, scale.z), Quaternion.IDENTITY);
						//component.Transform = trans;

						return heightfield;
					}
				default:
					throw new ApplicationException("ShapeComponent's Type was invalid!");
			}
		}


		/// <summary>
		/// Imports a collision shape from a .bullet file.
		/// </summary>
		/// <param name="name">Part of the filename. "media/physics/" + name + ".bullet"</param>
		/// <remarks>
		/// This only imports the first collision shape from the file. If it has multiple, they will be ignored.
		/// </remarks>
		public CollisionShape ImportCollisionShape(string name) {
			BulletWorldImporter importer = new BulletWorldImporter(LKernel.GetG<PhysicsMain>().World);

			// load that file
			if (importer.LoadFile(Settings.Default.BulletFileLocation + name + ".bullet")) {
				Launch.Log(string.Concat("[PhysicsMain] Importing ", Settings.Default.BulletFileLocation, name, ".bullet..."));
				// these should only have one collision shape in them, so we'll just use that
				return importer.GetCollisionShapeByIndex(0);
			}
			else {
				// if the file wasn't able to be loaded, throw an exception
				throw new IOException(Settings.Default.BulletFileLocation + name + ".bullet was unable to be imported!");
			}
		}

		/// <summary>
		/// Serializes a collision shape and exports a .bullet file.
		/// </summary>
		/// <param name="shape">The shape you want to serialize.</param>
		/// <param name="name">The name of the shape - this will be used as part of its filename. "media/physics/" + name + ".bullet"</param>
		public void SerializeShape(CollisionShape shape, string name) {
			Launch.Log(string.Concat("[PhysicsMain] Serializing new bullet mesh: ", Settings.Default.BulletFileLocation, name, ".bullet..."));
			// so we don't have to do this in the future, we make a .bullet file out of it
			DefaultSerializer serializer = new DefaultSerializer();
			serializer.StartSerialization();
			shape.SerializeSingleShape(serializer);
			serializer.FinishSerialization();
			var stream = serializer.LockBuffer();

			// export it
			using (var filestream = File.Create(Settings.Default.BulletFileLocation + name + ".bullet", serializer.CurrentBufferSize)) {
				stream.CopyTo(filestream);
				filestream.Close();
			}
			stream.Close();
		}

		public void Clear() {
			Shapes.Clear();
		}
	}
}
