using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Sound;
using Ponykart.Stuff;
using Ponykart.UI;

namespace Ponykart {
	public static partial class LKernel {
		private static IEnumerable<Type> LevelHandlerTypes;
		private static IEnumerable<Type> GlobalHandlerTypes;

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

			// physx stuff
			splash.Increment("Initialising physics engine, collision groups, and trigger area and contact reporters...");
			var phys = AddGlobalObject(new PhysicsMain());
			AddGlobalObject(new CollisionReporter());
			AddGlobalObject(new TriggerReporter());
			AddGlobalObject(new PhysicsMaterialManager());

			// sound stuff
			splash.Increment("Setting up sound system...");
			AddGlobalObject(new SoundMain());

			// level
			splash.Increment("Creating level...");
			AddGlobalObject(InitSceneManager(root));

			splash.Increment("Loading first level physics...");
			phys.LoadPhysicsLevel(Settings.Default.MainMenuName);

			splash.Increment("Creating player camera and viewport...");
			var playerCamera = AddLevelObject(new PlayerCamera());
			AddGlobalObject(InitViewport(renderWindow, playerCamera.Camera));

			// MOIS and input stuff
			splash.Increment("Starting input system...");
			AddGlobalObject(new InputMain());
			AddGlobalObject(new KeyBindingManager());
			AddGlobalObject(new InputSwallowerManager());
			AddGlobalObject(new Pauser());

			// spawner
			splash.Increment("Creating spawner...");
			AddGlobalObject(new WheelFactory());
			AddGlobalObject(new Spawner());

			// Miyagi and stuff
			splash.Increment("Initialising UI...");
			AddGlobalObject(new DebugOverlayManager());
			AddGlobalObject(new UIMain());
			AddGlobalObject(new DialogueManager());
			AddGlobalObject(new LuaConsoleManager());

			// lua
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
			AddGlobalObject(new PhysicsWrapper());
			lua.RunRegisterEvent();

			// this is a bit of a hack but it shouldn't matter much since we're only doing it once at the beginning
			splash.Increment("Spawning players...");
			AddGlobalObject(new PlayerManager());
			AddGlobalObject(new KartSpawnPositions());

			// handlers
			splash.Increment("Starting handlers...");
			SetUpHandlers();
			LoadGlobalHandlers();
			LoadLevelHandlers();

			levelManager.RunPostInitEvents();
		}

		static void SetUpHandlers() {
			var types = Assembly.GetExecutingAssembly().GetTypes();
			LevelHandlerTypes = types.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					.Where(a => a.Scope == HandlerScope.Level)
					.Count() > 0);
			GlobalHandlerTypes = types.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					.Where(a => a.Scope == HandlerScope.Global)
					.Count() > 0);
		}

		/// <summary>
		/// Load objects for each level
		/// This is called from LevelManager
		/// </summary>
		public static void LoadLevelObjects(LevelChangedEventArgs eventArgs) {
			
		}

		/// <summary>
		/// Load global handlers
		/// </summary>
		public static void LoadGlobalHandlers() {
			Launch.Log("[Loading] Initialising global handlers...");

			foreach (Type t in GlobalHandlerTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddGlobalObject(Activator.CreateInstance(t), t);
			}
		}

		/// <summary>
		/// Load handlers for each level
		/// </summary>
		public static void LoadLevelHandlers() {
			Launch.Log("[Loading] Initialising per-level handlers...");

			foreach (Type t in LevelHandlerTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddLevelObject(Activator.CreateInstance(t), t);
			}
			// the camera's a bit weird
			if (!Get<SceneManager>().HasCamera("Camera")) {
				var playerCam = AddLevelObject(new PlayerCamera());
				Get<Viewport>().Camera = playerCam.Camera;
			}
		}

		/// <summary>
		/// Unload and dispose of the special per-level handlers. This is run before the regular OnLevelUnload event.
		/// </summary>
		public static void UnloadLevelHandlers() {
			Launch.Log("[Loading] Disposing of level handlers...");
			foreach (var obj in LevelObjects.Values) {
				Console.WriteLine("[Loading] \tDisposing: " + obj.GetType().ToString());
				// if this cast fails, then you need to make sure the level handler implements ILevelHandler!
				(obj as ILevelHandler).Dispose();
			}
		}

		/// <summary>
		/// This is run just after the OnLevelUnload event.
		/// </summary>
		public static void Cleanup() {
			CleanSceneManagerThings();
			LevelObjects.Clear();
		}

		/// <summary>
		/// destroys everything in the scene manager so it's as good as new without destroying the scene manager itself
		/// </summary>
		private static void CleanSceneManagerThings() {
			Launch.Log("[Loading] Cleaning SceneManager...");
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
			var sceneMgr = root.CreateSceneManager(SceneType.ST_GENERIC, "sceneMgr");
			return sceneMgr;
		}

		private static Viewport InitViewport(RenderWindow window, Camera camera) {
			Launch.Log("[Loading] First Get<Viewport>");
			return window.AddViewport(camera);
		}
		#endregion

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
