using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lymph.Core;
using Lymph.Handlers;
using Lymph.IO;
using Lymph.Levels;
using Lymph.Lua;
using Lymph.Phys;
using Lymph.Sound;
using Lymph.Stuff;
using Lymph.UI;
using Mogre;

namespace Lymph {
	public static partial class LKernel {

		/// <summary>
		/// Load global objects on startup
		/// </summary>
		public static void LoadInitialObjects(Splash splash) {
			splash.Increment("Setting up Mogre core...");

			// this goes first since lots of things rely on it
			var levelManager = AddGlobalObject(new LevelManager());

			// mogre stuff
			var root		 = AddGlobalObject(InitRoot());
			var renderSystem = AddGlobalObject(InitRenderSystem(root));
			var renderWindow = AddGlobalObject(InitRenderWindow(root, Get<Main>(), renderSystem));

			splash.Increment("Initialising resources and resource groups...");
			InitResources();
			LoadResourceGroups();

			splash.Increment("Initialising physics engine, collision groups, and trigger area and contact reporters...");
			var physx = AddGlobalObject(new PhysXMain());
			AddGlobalObject(new TriggerReporter());
			AddGlobalObject(new ContactReporter());
			AddGlobalObject(new Groups());

			splash.Increment("Setting up sound system...");
			AddGlobalObject(new SoundMain());

			splash.Increment("Creating level...");
			var sceneManager = AddGlobalObject(InitSceneManager(root));
			var levelNode = sceneManager.RootSceneNode.CreateChildSceneNode("RootLevelNode", new Vector3(0, 0, 0));
			var DSL = AddLevelObject(new DotSceneLoader());
			DSL.ParseDotScene(Settings.Default.FirstLevelName + ".scene", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, levelNode);

			splash.Increment("Loading first level physics...");
			physx.LoadPhysicsLevel(Settings.Default.FirstLevelName);

			splash.Increment("Creating player camera and viewport...");
			var playerCamera = AddLevelObject(new PlayerCamera());
			AddGlobalObject(InitViewport(renderWindow, playerCamera));

			splash.Increment("Starting input system...");
			AddGlobalObject(new InputMain());
			AddGlobalObject(new InputSwallowerManager());
			AddGlobalObject(new Pauser());

			splash.Increment("Creating spawner...");
			var spawner = AddGlobalObject(new Spawner());

			splash.Increment("Initialising UI...");
			AddGlobalObject(new DebugOverlayManager());
			AddGlobalObject(new UIMain());
			AddGlobalObject(new DialogueManager());
			AddGlobalObject(new LuaConsoleManager());

			splash.Increment("Setting up scripting engine...");
			var lua = AddGlobalObject(new LuaMain());
			AddGlobalObject(new LKernelWrapper());
			AddGlobalObject(new PauserWrapper());
			AddGlobalObject(new LevelManagerWrapper());
			AddGlobalObject(new TriggerWrapper());
			AddGlobalObject(new SoundWrapper());
			AddGlobalObject(new SpawnerWrapper());
			AddGlobalObject(new LevelWrapper());
			AddGlobalObject(new IOWrapper());
			lua.RunRegisterEvent();

			// this is a bit of a hack but it shouldn't matter much since we're only doing it once at the beginning
			splash.Increment("Spawning player...");
			// the player adds itself to the kernel
			var playerImporter = new PlayerImporter();
			spawner.Spawn(playerImporter.Parse());

			splash.Increment("Starting handlers...");
			AddLevelObject(new DialogueTest());
			AddGlobalObject(new EscHandler());
			AddGlobalObject(new LevelChangerHandler());
			AddGlobalObject(new LevelUIHandler());
			AddGlobalObject(new LoadingUIHandler());
			AddGlobalObject(new MiscKeyboardHandler());
			AddLevelObject(new MovementHandler());
			AddLevelObject(new PlayerMovementHandler());
			AddGlobalObject(new PauseUIHandler());
			AddLevelObject(new TriggerRegionsTest());

			levelManager.RunPostInitEvents();
		}

		/// <summary>
		/// Load objects for each level
		/// This is called from LevelManager
		/// </summary>
		public static void LoadLevelObjects(LevelChangedEventArgs eventArgs) {

			// this has to be in the kernel because multiple things use it
			AddLevelObject(new DotSceneLoader());

			Launch.Log("[Loading] Creating player...");
			var playerCamera = AddLevelObject(new PlayerCamera());
			Get<Viewport>().Camera = playerCamera.Camera;
		}

		/// <summary>
		/// Load handlers for each level
		/// Have to load these separately from LoadLevelObjects because these depend on some Things (such as the player)
		/// </summary>
		public static void LoadLevelHandlers() {
			Launch.Log("[Loading] Initialising per-level handlers...");
			AddLevelObject(new DialogueTest());
			AddLevelObject(new MovementHandler());
			AddLevelObject(new PlayerMovementHandler());
			AddLevelObject(new TriggerRegionsTest());
		}

		/// <summary>
		/// unload and dispose of the special per-level objects
		/// </summary>
		public static void UnloadLevelObjects() {
			Launch.Log("[Loading] Destroying everything in scene...");
			foreach (var obj in LevelObjects.Values.OfType<IDisposable>()) {
				Console.WriteLine("[Loading] Disposing: " + obj.GetType().ToString());
				obj.Dispose();
			}
			CleanSceneManagerThings();
			LevelObjects.Clear();
		}

		#region special initialisers
		private static Root InitRoot() {
			return new Root("plugins.cfg", "", "Ogre.log");
		}

		private static RenderSystem InitRenderSystem(Root root) {
			Launch.Log("[Loading] First Get<RenderSystem>");

			if (root.RenderSystem == null) {
				var renderSystem = root.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
				renderSystem.SetConfigOption("Full Screen", "No");
				renderSystem.SetConfigOption("Video Mode", Constants.WINDOW_WIDTH + " x " + Constants.WINDOW_HEIGHT + " @ 32-bit colour");

				root.RenderSystem = renderSystem;
			}

			return root.RenderSystem;
		}

		private static RenderWindow InitRenderWindow(Root root, Form form, RenderSystem renderSystem) {
			Launch.Log("[Loading] First Get<RenderWindow>");

			//Ensure RenderSystem has been Initialised
			root.RenderSystem = renderSystem;

			root.Initialise(false, "Lymph");
			NameValuePairList miscParams = new NameValuePairList();

			if (form.Handle != IntPtr.Zero)
				miscParams["externalWindowHandle"] = form.Handle.ToString();

			miscParams["vsync"] = "true";    // by Ogre default: false

			return root.CreateRenderWindow("Lymph main RenderWindow", Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT, false, miscParams);
		}

		private static SceneManager InitSceneManager(Root root) {
			Launch.Log("[Loading] First Get<SceneManager>");
			/*try {
				var currentSceneManager = root.GetSceneManager("sceneMgr");
				root.DestroySceneManager(currentSceneManager);
			} catch { }*/
			return root.CreateSceneManager(SceneType.ST_GENERIC, "sceneMgr");
		}

		private static Viewport InitViewport(RenderWindow window, PlayerCamera playerCamera) {
			Launch.Log("[Loading] First Get<Viewport>");
			return window.AddViewport(playerCamera.Camera);
		}
		#endregion

		/// <summary>
		/// destroys everything in the scene manager so it's as good as new without destroying the scene manager itself
		/// </summary>
		private static void CleanSceneManagerThings() {
			Launch.Log("Cleaning SceneManager...");
			var sceneMgr = Get<SceneManager>();

			sceneMgr.DestroyAllAnimations();
			sceneMgr.DestroyAllAnimationStates();
			sceneMgr.DestroyAllBillboardChains();
			sceneMgr.DestroyAllBillboardSets();
			sceneMgr.DestroyAllCameras();
			sceneMgr.DestroyAllEntities();
			sceneMgr.DestroyAllInstancedGeometry();
			sceneMgr.DestroyAllLights();
			sceneMgr.DestroyAllManualObjects();
			sceneMgr.DestroyAllMovableObjects();
			sceneMgr.DestroyAllParticleSystems();
			sceneMgr.DestroyAllRibbonTrails();
			sceneMgr.DestroyAllStaticGeometry();
			sceneMgr.RootSceneNode.RemoveAndDestroyAllChildren(); // how morbid
		}

		/// <summary>
		/// Basically adds all of the resource locations but doesn't actually load anything.
		/// </summary>
		private static void InitResources() {
			ConfigFile file = new ConfigFile();
			file.Load("resources.cfg", "\t:=", true);
			ConfigFile.SectionIterator sectionIterator = file.GetSectionIterator();

			while (sectionIterator.MoveNext()) {
				string currentKey = sectionIterator.CurrentKey;
				foreach (KeyValuePair<string, string> pair in sectionIterator.Current) {
					string key = pair.Key;
					string name = pair.Value;
					ResourceGroupManager.Singleton.AddResourceLocation(name, key, currentKey);
				}
			}
		}

		/// <summary>
		/// This is where resources are actually loaded into memory. In a game with lots of files
		/// you want to group them and load them as necessary - right now this method just loads
		/// everything at once.
		/// </summary>
		private static void LoadResourceGroups() {
			TextureManager.Singleton.DefaultNumMipmaps = 1;
			// vvvvvvv this vvvvvvv
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
		}
	}
}
