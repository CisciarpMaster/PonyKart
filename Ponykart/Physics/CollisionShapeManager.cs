using System;
using System.Collections.Generic;
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
					shape = ImportCollisionShape(filename.Replace(".bullet", string.Empty));
				}
				else {
					Launch.Log("[PhysicsMain] " + filename + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");
					// it does not have a file, so we need to convert our ogre mesh
					shape = new BvhTriangleMeshShape(OgreToBulletMesh.Convert(ent, node), true, true);
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
			CollisionShape componentShape;

			switch (component.Type) {
				case ThingEnum.Box:
					componentShape = new BoxShape(component.Dimensions);
					break;
				case ThingEnum.Cylinder:
					componentShape = new CylinderShape(component.Dimensions);
					break;
				case ThingEnum.Cone:
					componentShape = new ConeShape(component.Radius, component.Height);
					(componentShape as ConeShape).ConeUpIndex = 1;
					break;
				case ThingEnum.Capsule:
					componentShape = new CapsuleShape(component.Radius, component.Height);
					break;
				case ThingEnum.Sphere:
					componentShape = new SphereShape(component.Radius);
					break;
				case ThingEnum.Hull:
					var sceneMgr = LKernel.GetG<SceneManager>();
					// get our entity if we have one, create it if we don't
					Entity ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

					TriangleMesh trimesh = OgreToBulletMesh.Convert(ent.GetMesh(),
																	component.Transform.GetTrans(),
																	component.Transform.ExtractQuaternion(),
																	Vector3.UNIT_SCALE);
					componentShape = new ConvexTriangleMeshShape(trimesh);

					// TODO: figure out how to deal with concave triangle mesh shapes since apparently they aren't being exported
					//LKernel.GetG<PhysicsMain>().SerializeShape(Shape, name);
					break;
				case ThingEnum.Mesh:
					// example
					// physics/example.bullet
					string name, bulletFilePath;

					if (component.Mesh.EndsWith(".mesh")) {
						name = component.Mesh.Remove(component.Mesh.IndexOf(".mesh"));
						bulletFilePath = Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension;
					}
					else if (component.Mesh.EndsWith(Settings.Default.BulletFileExtension)) {
						name = component.Mesh.Remove(component.Mesh.IndexOf(Settings.Default.BulletFileExtension));
						bulletFilePath = Settings.Default.BulletFileLocation + component.Mesh;
					}
					else {
						throw new ApplicationException("Your \"Mesh\" property needs to end in either .mesh or .bullet!");
					}

					// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
					if (File.Exists(bulletFilePath)) {
						// so it has a file
						componentShape = ImportCollisionShape(name);
					}
					else {
						Launch.Log("[CollisionShapeManager] " + bulletFilePath + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");

						// it does not have a file, so we need to convert our ogre mesh
						sceneMgr = LKernel.GetG<SceneManager>();
						ent = sceneMgr.HasEntity(component.Mesh) ? sceneMgr.GetEntity(component.Mesh) : sceneMgr.CreateEntity(component.Mesh, component.Mesh);

						componentShape = new BvhTriangleMeshShape(
							OgreToBulletMesh.Convert(ent.GetMesh(),
													 component.Transform.GetTrans(),
													 component.Transform.ExtractQuaternion(),
													 Vector3.UNIT_SCALE),
							true, true);

						// and then export it as a .bullet file
						SerializeShape(componentShape, name);
					}
					break;

				default:
					throw new ApplicationException("ShapeComponent's Type was invalid!");
			}

			return componentShape;
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
			if (importer.LoadFile(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension)) {
				Launch.Log(string.Concat("[PhysicsMain] Importing ", Settings.Default.BulletFileLocation, name, Settings.Default.BulletFileExtension, "..."));
				// these should only have one collision shape in them, so we'll just use that
				return importer.GetCollisionShapeByIndex(0);
			}
			else {
				// if the file wasn't able to be loaded, throw an exception
				throw new IOException(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension + " was unable to be imported!");
			}
		}

		/// <summary>
		/// Serializes a collision shape and exports a .bullet file.
		/// </summary>
		/// <param name="shape">The shape you want to serialize.</param>
		/// <param name="name">The name of the shape - this will be used as part of its filename. "media/physics/" + name + ".bullet"</param>
		public void SerializeShape(CollisionShape shape, string name) {
			Launch.Log(string.Concat("[PhysicsMain] Serializing new bullet mesh: ", Settings.Default.BulletFileLocation, name, Settings.Default.BulletFileExtension, "..."));
			// so we don't have to do this in the future, we make a .bullet file out of it
			DefaultSerializer serializer = new DefaultSerializer();
			serializer.StartSerialization();
			shape.SerializeSingleShape(serializer);
			serializer.FinishSerialization();
			var stream = serializer.LockBuffer();

			// export it
			using (var filestream = File.Create(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension, serializer.CurrentBufferSize)) {
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
