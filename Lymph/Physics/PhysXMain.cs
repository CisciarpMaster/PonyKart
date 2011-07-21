using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mogre;
using Mogre.PhysX;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Stuff;
using Actor = Mogre.PhysX.Actor;

namespace Ponykart.Phys {
	public partial class PhysXMain : IDisposable {
		private Physics physics;
		private Scene scene;
		private bool quit = false;
		public ControllerManager ControllerManager { get; set; }

		private float update, update10, elapsed;
		// TODO: do something with this
		public static Mogre.PhysX.Material noFrictionMaterial;

		/// <summary>
		/// our collection of things to dispose; these are processed after every "frame"
		/// </summary>
		public ICollection<Thing> ThingsToDispose { get; private set; }

		public PhysXMain() {
			Launch.Log("[Loading] Creating PhysXMain...");

			physics = Physics.Create();
			physics.Parameters.SkinWidth = 0.01f; // how far we let objects penetrate each other
			physics.Parameters.DefaultSleepLinVelSquared = 0.1f;
			physics.Parameters.DefaultSleepAngVelSquared = 0.1f;

			physics.RemoteDebugger.Connect("localhost"); // connect to the debugger

			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;
			LKernel.Get<Root>().FrameStarted += FrameStarted;
			LKernel.Get<Root>().FrameEnded += FrameEnded;

			this.update = 1f / Constants.PH_FRAMERATE;
			this.update10 = update * 10f;
			this.elapsed = 0f;

			ThingsToDispose = new Collection<Thing>();

			ControllerManager = physics.ControllerManager;

			Launch.Log("[Loading] PhysXMain created!");
		}

		/// <summary>
		/// Sets up a new physics scene.
		/// 
		/// This is called manually from LevelManager at the moment because the Scene needs to be set up before anything else is put in it.
		/// </summary>
		/// <param name="eventArgs"></param>
		public void LoadPhysicsLevel(string levelName) {
			Launch.Log("[Loading] Setting up PhysX scene");
			Launch.Log("*** If you get a PhysX error here about CUDA, ignore it! ***");

			CreateScene(levelName);

			// Sets default material
			var defaultMaterial = scene.Materials[0];
			// restitution = bounciness
			defaultMaterial.Restitution = 0.5f;
			defaultMaterial.DynamicFriction = 0.6f;
			defaultMaterial.StaticFriction = 0.6f;

			// TODO
			var tyreMaterialDesc = new MaterialDesc();
			tyreMaterialDesc.Flags |= MaterialFlags.DisableFriction;
			noFrictionMaterial = scene.CreateMaterial(tyreMaterialDesc);


			// creates collision meshes out of static objects
			// get the scene manager
			SceneManager sceneMgr = LKernel.Get<SceneManager>();
			// create a node that will be the root of all of these static level meshes
			SceneNode levelNode = sceneMgr.RootSceneNode.CreateChildSceneNode("RootLevelNode", new Vector3(0, 0, 0));
			// parse our .scene file
			DotSceneLoader dsl = new DotSceneLoader();
			dsl.ParseDotScene(levelName + ".scene", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, levelNode);
			// then go through each of the static objects and turn them into trimeshes.
			/*
			 * TODO: neoaxis had some sort of file format just for these trimeshes - look into it? might speed up loading times
			 * and make it easier to have a separate, more simple mesh just for the physics world.
			 */
			foreach (string s in dsl.StaticObjects) {
				Entity dslEnt = sceneMgr.GetEntity(s);
				SceneNode dslNode = sceneMgr.GetSceneNode(s);


				//http://forums.create.msdn.com/forums/p/35858/207315.aspx
				//http://www.ogre3d.org/tikiwiki/tiki-index.php?page=Raycasting+to+the+polygon+level+-+Mogre#GetMeshInformation


				TriangleMeshShapeDesc tmsd = new TriangleMeshShapeDesc();
				tmsd.Group = Groups.CollidableNonPushableID;
				ActorDesc dslActorDesc = new ActorDesc();

				MakeTriangleMesh(dslEnt, dslNode, out tmsd, out dslActorDesc);

				Actor dslActor = scene.CreateActor(dslActorDesc);
				dslActor.Name = dslNode.Name;
			}
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			// we need to get rid of all of the controllers before we dispose of the scene, due to a bug in the wrapper
			// see http://www.ogre3d.org/addonforums/viewtopic.php?f=8&t=14175
			/*foreach (Controller c in ControllerManager.Controllers) {
				if (!c.IsDisposed)
					c.Dispose();
			}
			ControllerManager.PurgeControllers();*/
			// for whatever reason that special physx wrapper dll smoove gave me did something so the scene disposes itself.
			// Or something. Man I dunno. But if we try to dispose it then it crashes, saying it's already disposed, soo...
			if (!scene.IsDisposed)
				scene.Dispose();
			ThingsToDispose.Clear();
		}

		/// <summary>
		/// Creates a physx SceneDesc, sets its properties, then creates a Scene from the SceneDesc
		/// </summary>
		/// <param name="levelName"></param>
		void CreateScene(string levelName)
		{
			// Create a descriptor
			SceneDesc sDesc = new SceneDesc();

			// set some basic stuff like gravity, bounds, which way is up, etc
			sDesc.Gravity = new Vector3(0, Constants.GRAVITY, 0);
			sDesc.MaxBounds = new AxisAlignedBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));
			sDesc.UpAxis = 1;

			// timing stuff
			sDesc.TimeStepMethod = TimeStepMethods.Fixed;
			sDesc.MaxTimeStep = 1f / Constants.PH_FRAMERATE;
			sDesc.MaxIterationCount = 10;

			// reporters
			sDesc.UserContactReport = LKernel.Get<ContactReporter>();
			sDesc.UserTriggerReport = LKernel.Get<TriggerReporter>();

			scene = physics.CreateScene(sDesc);
		}

		/// <summary>
		/// Runs every frame. Simulates one frame of physics and then updates mogre nodes and stuff.
		/// 
		/// Also has the speed limiting for lymph and camera updating.
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (Pauser.IsPaused || !LKernel.Get<LevelManager>().IsValidLevel)
				return true;

			elapsed += evt.timeSinceLastFrame;
			if (elapsed > update && elapsed < update10) {
				while (elapsed > update) {
					ControllerManager.UpdateControllers();
					scene.Simulate(update);
					scene.FlushStream();
					scene.FetchResults(SimulationStatuses.AllFinished, true);
					elapsed -= update;
				}
			} else if (elapsed < update) {
				// not enough time has passed this loop, so ignore for now
			} else {
				// too much time has passed (would require more than 10 updates!), so just update once and reset.
				// this often happens on the first frame of a game, where assets and other things were loading, then
				// the elapsed time since the last drawn frame is very long.
				ControllerManager.UpdateControllers();
				scene.Simulate(elapsed);
				scene.FlushStream();
				scene.FetchResults(SimulationStatuses.AllFinished, true);
				elapsed = 0; // reset the elapsed time so we don't become "eternally behind".
			}
			
			// move the player
			//Actor lymphactor = LKernel.Get<Player>().Actor;
			//lymphactor.AddForce(lymphactor.LinearVelocity * -1f, ForceModes.Force);

			SceneManager sceneMgr = LKernel.Get<SceneManager>();
			// update all nodes
			foreach (Actor a in scene.Actors) {
				// no need to update static actors
				if (!a.IsDynamic || a.Name == null) continue;

				// only loop through actors that have an associated node
				if (sceneMgr.HasSceneNode(a.Name)) {
					var mat = a.GlobalPose;
					var rot = a.GlobalOrientationQuaternion;

					// if the thing does not have userdata, get the scene node from the scene manager
					// otherwise cast its userdata to an actor and then
					var tempnode = a.UserData == null ? sceneMgr.GetSceneNode(a.Name) : (a.UserData as Thing).Node;

					tempnode.Orientation = rot;
					tempnode.Position = mat.GetTrans();

				}
			}
			// update the camera after all of the physics stuff happened
			LKernel.Get<PlayerCamera>().UpdateCamera(evt);

			return !quit;
		}

		/// <summary>
		/// Dispose everything that's waiting to be disposed. We don't want to do this while the physics engine is in progress!
		/// </summary>
		/// <param name="evt"></param>
		/// <returns></returns>
		bool FrameEnded(FrameEvent evt) {
			if (ThingsToDispose.Count > 0) {
				foreach (Thing t in ThingsToDispose) {
					t.Dispose();
				}
				ThingsToDispose.Clear();
			}
			
			return !quit;
		}
		
		public void ShootBox() {
			Vector3 pos = LKernel.Get<PlayerManager>().MainPlayer.ActorPosition;

			BoxShapeDesc bsd = new BoxShapeDesc(new Vector3(0.5f, 0.5f, 0.5f));
			bsd.Group = Groups.CollidablePushableID;

			ActorDesc ad = new ActorDesc(new BodyDesc(), 1, bsd);
			ad.GlobalPosition = pos;

			string name = "Box_" + (IDs.New);
			Actor a = scene.CreateActor(ad);
			a.Name = name;
			//a.BodyFlags.FrozenPosY = true;
			a.LinearDamping = 0.1f;
			a.AngularDamping = 0.1f;
			a.BodyFlags.EnergySleepTest = true;

			SceneManager sceneMgr = LKernel.Get<SceneManager>();
			Entity ent = sceneMgr.CreateEntity(name, "primitives/box.mesh");
			ent.SetMaterialName("brick");
			SceneNode node = sceneMgr.RootSceneNode.CreateChildSceneNode(name, pos);
			node.AttachObject(ent);
			node.SetScale(1, 1, 1);
		}

		public Scene Scene {
			get { return scene; }
		}

		public void Dispose() {
			physics.Dispose();
			Environment.Exit(0);
		}
	}
}
