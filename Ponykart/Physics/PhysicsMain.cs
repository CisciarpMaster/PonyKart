using System.Collections.Generic;
using System.Collections.ObjectModel;
using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
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

			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;

			ThingsToDispose = new Collection<LThing>();

			Launch.Log("[Loading] PhysicsMain created!");
		}

		/// <summary>
		/// Sets up a new physics scene.
		/// 
		/// This is called manually from LevelManager at the moment because the Scene needs to be set up before anything else is put in it.
		/// </summary>
		/// <param name="eventArgs"></param>
		public void LoadPhysicsLevel(string levelName) {
			Launch.Log("[Loading] Setting up Physics scene");

			CreateWorld(levelName);

			// creates collision meshes out of static objects
			// get the scene manager
			SceneManager sceneMgr = LKernel.Get<SceneManager>();
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

				var shape = new BvhTriangleMeshShape(OgreToBulletMesh.Convert(dslEnt, dslNode), true, true);
				var info = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape, Vector3.ZERO);
				var body = new RigidBody(info);
				body.SetCollisionGroup(PonykartCollisionGroups.Environment);
				body.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
				body.SetName(dslNode.Name);
				world.AddRigidBody(body, PonykartCollisionGroups.Environment, PonykartCollidesWithGroups.Environment);
			}

			// make an infinite plane so we don't fall forever. TODO: hook up an event so when we collide with this, we respawn back on the track
			Matrix4 matrix = new Matrix4();
			matrix.MakeTransform(new Vector3(0, -10, 0), Vector3.UNIT_SCALE, new Quaternion(0, 0, 0, 1));
			var groundinfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(matrix), new StaticPlaneShape(Vector3.NEGATIVE_UNIT_Y, 1), Vector3.ZERO);
			var groundBody = new RigidBody(groundinfo);
			groundBody.SetName("ground");
			groundBody.SetCollisionGroup(PonykartCollisionGroups.Environment);
			groundBody.CollisionFlags = CollisionFlags.StaticObject | CollisionFlags.DisableVisualizeObject;
			world.AddRigidBody(groundBody, PonykartCollisionGroups.Environment, PonykartCollidesWithGroups.Environment);

			if (PostCreateWorld != null)
				PostCreateWorld(world);

			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			LKernel.Get<Root>().FrameEnded -= FrameEnded;

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
			if (Pauser.IsPaused || !LKernel.Get<LevelManager>().IsPlayableLevel || !LKernel.Get<LevelManager>().IsValidLevel || world.IsDisposed)
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
