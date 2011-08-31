using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using BulletSharp;
using BulletSharp.Serialize;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.Stuff;
using IDisposable = System.IDisposable;

namespace Ponykart.Physics {
	public delegate void PhysicsWorldEventHandler(DiscreteDynamicsWorld world);
	public delegate void PhysicsSimulateEventHandler(DiscreteDynamicsWorld world, FrameEvent evt);

	public class PhysicsMain : IDisposable {
		private bool quit = false;

		private BroadphaseInterface broadphase;
		private DefaultCollisionConfiguration dcc;
		private CollisionDispatcher dispatcher;
		private SequentialImpulseConstraintSolver solver;

		private DiscreteDynamicsWorld world;

		/// <summary>
		/// This is invoked after every physics "tick", which occur multiple times per frame.
		/// Want to change a speed based on a maximum velocity or whatever? Do it with this.
		/// </summary>
		public event DynamicsWorld.InternalTickCallback PostTick;
		/// <summary>
		/// Is invoked right after the physics world is created.
		/// </summary>
		public event PhysicsWorldEventHandler PostCreateWorld;
		/// <summary>
		/// Is invoked right before the physics world is simulated.
		/// </summary>
		public event PhysicsSimulateEventHandler PreSimulate;
		/// <summary>
		/// Is invoked right after the physics world is simulated.
		/// </summary>
		public event PhysicsSimulateEventHandler PostSimulate;

		/// <summary>
		/// Should we draw debug lines or not?
		/// </summary>
		public static bool DrawLines = 
#if DEBUG
			false;
#else
			false;
#endif

		/// <summary>
		/// our collection of things to dispose; these are processed after every "frame"
		/// </summary>
		public ICollection<LThing> ThingsToDispose { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PhysicsMain() {
			Launch.Log("[Loading] Creating PhysicsMain...");

			LKernel.GetG<LevelManager>().OnLevelUnload += OnLevelUnload;

			ThingsToDispose = new Collection<LThing>();

			Launch.Log("[Loading] PhysicsMain created!");
		}

		/// <summary>
		/// Sets up a new physics scene.
		/// 
		/// This is called manually from LevelManager at the moment because the Scene needs to be set up before anything else is put in it.
		/// </summary>
		public void LoadPhysicsLevel(string levelName) {
			Launch.Log("[Loading] Setting up Physics scene");

			CreateWorld(levelName);

			// creates collision meshes out of static objects
			// get the scene manager
			SceneManager sceneMgr = LKernel.GetG<SceneManager>();
			// create a node that will be the root of all of these static level meshes
			SceneNode levelNode = sceneMgr.RootSceneNode.CreateChildSceneNode("RootLevelNode", new Vector3(0, 0, 0));
			// parse our .scene file
			DotSceneLoader dsl = new DotSceneLoader();
			dsl.ParseDotScene(levelName + ".scene", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, levelNode);

			// then go through each of the static objects and turn them into trimeshes.
			foreach (string s in dsl.StaticObjects) {
				// I have NO IDEA what the fuck fixed this problem, but hey, it's fixed now!
				Entity dslEnt = sceneMgr.GetEntity(s);
				SceneNode dslNode = sceneMgr.GetSceneNode(s);

				CollisionShape shape;

				string bulletFilePath = Settings.Default.BulletFileLocation + dslNode.Name + Settings.Default.BulletFileExtension;

				// right, so what we do is test to see if this shape has a .bullet file, and if it doesn't, create one
				if (File.Exists(bulletFilePath)) {
					// so it has a file
					Launch.Log("[PhysicsMain] Loading " + bulletFilePath + "...");
					shape = ImportCollisionShape(dslNode.Name);
				}
				else {
					Launch.Log("[PhysicsMain] " + bulletFilePath + " does not exist, converting Ogre mesh into physics trimesh and exporting new .bullet file...");
					// it does not have a file, so we need to convert our ogre mesh
					shape = new BvhTriangleMeshShape(OgreToBulletMesh.Convert(dslEnt, dslNode), true, true);
					// and then export it as a .bullet file
					SerializeShape(shape, dslNode.Name);
				}

				// then do the rest as usual
				var info = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape, Vector3.ZERO);
				var body = new RigidBody(info);
				body.SetCollisionGroup(PonykartCollisionGroups.Environment);
				body.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
				body.SetName(dslNode.Name);
				world.AddRigidBody(body, PonykartCollisionGroups.Environment, PonykartCollidesWithGroups.Environment);
			}

			// make a ground plane for us
			CreateGroundPlane(-10);

			// run some events
			if (PostCreateWorld != null)
				PostCreateWorld(world);

			LKernel.GetG<Root>().FrameEnded += FrameEnded;
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;

			if (!world.IsDisposed)
				world.Dispose();
			ThingsToDispose.Clear();
		}

		/// <summary>
		/// Creates the world
		/// </summary>
		void CreateWorld(string levelName) {
			Launch.Log("[PhysicsMain] Creating new world...");
			// have to make more of these every level because disposing the world apparently disposes of them too.
			broadphase = new DbvtBroadphase();
			dcc = new DefaultCollisionConfiguration();
			dispatcher = new CollisionDispatcher(dcc);
			solver = new SequentialImpulseConstraintSolver();

			world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, dcc);

			world.Gravity = new Vector3(0, Constants.GRAVITY, 0);
			// TODO: this isn't working for some reason
			world.SetInternalTickCallback(PostTick);
		}

		/// <summary>
		/// Runs just before every frame. Simulates one frame of physics.
		/// Physics simulation should be the only thing that's using FrameEnded!
		/// </summary>
		bool FrameEnded(FrameEvent evt) {
			if (Pauser.IsPaused || !LKernel.GetG<LevelManager>().IsPlayableLevel || !LKernel.GetG<LevelManager>().IsValidLevel || world.IsDisposed)
				return true;

			// dispose of everything waiting to be disposed
			if (ThingsToDispose.Count > 0) {
				foreach (LThing t in ThingsToDispose) {
					t.Dispose();
				}
				ThingsToDispose.Clear();
			}

			// run the events that go just before we simulate
			if (PreSimulate != null)
				PreSimulate(world, evt);

			world.StepSimulation(evt.timeSinceLastFrame, Constants.PH_MAX_SUBSTEPS, Constants.PH_FIXED_TIMESTEP);

			// run the events that go just after we simulate
			if (PostSimulate != null)
				PostSimulate(world, evt);

			if (DrawLines)
				world.DebugDrawWorld();

			return !quit;
		}

		/// <summary>
		/// Create a static ground plane facing upwards.
		/// </summary>
		/// <param name="yposition">The Y position that the plane is located at.</param>
		void CreateGroundPlane(float yposition) {
			// make an infinite plane so we don't fall forever. TODO: hook up an event so when we collide with this, we respawn back on the track
			Matrix4 matrix = new Matrix4();
			matrix.MakeTransform(new Vector3(0, yposition, 0), Vector3.UNIT_SCALE, new Quaternion(0, 0, 0, 1));

			var groundinfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(matrix), new StaticPlaneShape(Vector3.NEGATIVE_UNIT_Y, 1), Vector3.ZERO);
			var groundBody = new RigidBody(groundinfo);
			groundBody.SetName("ground");
			groundBody.SetCollisionGroup(PonykartCollisionGroups.Environment);
			groundBody.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
			world.AddRigidBody(groundBody, PonykartCollisionGroups.Environment, PonykartCollidesWithGroups.Environment);
		}

		/// <summary>
		/// Serializes a collision shape and exports a .bullet file.
		/// </summary>
		/// <param name="shape">The shape you want to serialize.</param>
		/// <param name="name">The name of the shape - this will be used as part of its filename.</param>
		public void SerializeShape(CollisionShape shape, string name) {
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

		/// <summary>
		/// Imports a collision shape from a .bullet file.
		/// </summary>
		/// <param name="name">Part of the filename. "media/physics/" + name + ".bullet"</param>
		/// <returns>The collision shape from the file</returns>
		/// <remarks>
		/// This only imports the first collision shape from the file. If it has multiple, they will be ignored.
		/// </remarks>
		public CollisionShape ImportCollisionShape(string name) {
			BulletWorldImporter importer = new BulletWorldImporter(world);
			// load that file
			if (importer.LoadFile(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension)) {
				// these should only have one collision shape in them, so we'll just use that
				return importer.GetCollisionShapeByIndex(0);
			}
			else
				// if the file wasn't able to be loaded, throw an exception
				throw new IOException(Settings.Default.BulletFileLocation + name + Settings.Default.BulletFileExtension + " was unable to be imported!");
		}

		public DiscreteDynamicsWorld World {
			get { return world; }
		}

		public void Dispose() {
			broadphase.Dispose();
			dcc.Dispose();
			dispatcher.Dispose();
			solver.Dispose();
		}
	}
}
