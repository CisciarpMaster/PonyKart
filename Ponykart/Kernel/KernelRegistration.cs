using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Sound;
using Ponykart.Stuff;
using Ponykart.UI;
using PonykartParsers;

namespace Ponykart {
	public static partial class LKernel {
		private static IEnumerable<Type> LevelHandlerTypes;
		private static IEnumerable<Type> GlobalHandlerTypes;
		private static IEnumerable<Type> LuaWrapperTypes;

		/// <summary>
		/// Load global objects on startup
		/// </summary>
		public static void LoadInitialObjects(Splash splash) {
			// this goes first since lots of things rely on it
			splash.Increment("Setting up level manager...");
			var levelManager = AddGlobalObject(new LevelManager());

			// mogre stuff
			splash.Increment("Initialising Mogre core...");
			var root		 = AddGlobalObject(InitRoot());
			splash.Increment("Initialising render system...");
			var renderSystem = AddGlobalObject(InitRenderSystem(root));
			splash.Increment("Creating render window...");
			var renderWindow = AddGlobalObject(InitRenderWindow(root, GetG<Main>(), renderSystem));

			splash.Increment("Initialising resources and resource groups...");
			InitResources();
			LoadResourceGroups();

			// physx stuff
			splash.Increment("Initialising Bullet physics engine, collision reporter, trigger region reporter, and contact reporter...");
			try {
				AddGlobalObject(new PhysicsMain());
				AddGlobalObject(new CollisionReporter());
				AddGlobalObject(new TriggerReporter());
				AddGlobalObject(new PhysicsMaterialManager());
				AddGlobalObject(new PhysicsMaterialFactory());
			}
			catch {
				MessageBox.Show("BulletSharp loading unsuccessful! Try installing a 2010 VC++ Redistributable (google it)!", "Well, shit.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			// sound stuff
			splash.Increment("Setting up sound system...");
			AddGlobalObject(new SoundMain());

			// level
			splash.Increment("Creating scene manager...");
			AddGlobalObject(InitSceneManager(root));

			splash.Increment("Loading first level physics...");
			GetG<PhysicsMain>().LoadPhysicsLevel(Settings.Default.MainMenuName);

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
			AddGlobalObject(new ThingDatabase());
			AddGlobalObject(new Spawner());

			// Miyagi and stuff
			splash.Increment("Initialising Miyagi...");
			AddGlobalObject(new DebugOverlayManager());
			AddGlobalObject(new UIMain());
			AddGlobalObject(new LuaConsoleManager());

			// lua needs these
			splash.Increment("Registering handlers and wrappers...");
			SetUpHandlers();

			// lua
			splash.Increment("Setting up Lua engine and wrappers...");
			var lua = AddGlobalObject(new LuaMain());
			LoadLuaWrappers();
			lua.RunRegisterEvent();

			// players
			splash.Increment("Spawning players...");
			AddGlobalObject(new PlayerManager());

			// handlers
			splash.Increment("Loading global handlers...");
			LoadGlobalHandlers();
			splash.Increment("Loading level handlers...");
			LoadLevelHandlers(LevelType.Menu);



			splash.Increment("Running post-initialisation events...");
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
			LuaWrapperTypes = types.Where(
				t => ((LuaPackageAttribute[]) t.GetCustomAttributes(typeof(LuaPackageAttribute), false))
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
		static void LoadGlobalHandlers() {
			Launch.Log("[Loading] Initialising global handlers...");

			foreach (Type t in GlobalHandlerTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddGlobalObject(Activator.CreateInstance(t), t);
			}
		}

		/// <summary>
		/// Load handlers for each level
		/// </summary>
		public static void LoadLevelHandlers(LevelType newLevelType) {
			Launch.Log("[Loading] Initialising per-level handlers...");

			IEnumerable<Type> e = LevelHandlerTypes.Where(
				t => ((HandlerAttribute[]) t.GetCustomAttributes(typeof(HandlerAttribute), false))
					 .Where(a => a.LevelType.HasFlag(newLevelType))
					 .Count() > 0);

			foreach (Type t in e) {
				Launch.Log("[Loading] \tCreating " + t);
				AddLevelObject(Activator.CreateInstance(t), t);
			}
			// the camera's a bit weird
			if (!GetG<SceneManager>().HasCamera("Camera")) {
				var playerCam = AddLevelObject(new PlayerCamera());
				GetG<Viewport>().Camera = playerCam.Camera;
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
				(obj as ILevelHandler).Detach();
			}
		}

		/// <summary>
		/// Load lua wrappers
		/// </summary>
		static void LoadLuaWrappers() {
			Launch.Log("[Loading] Initialising lua wrappers...");

			foreach (Type t in LuaWrapperTypes) {
				Launch.Log("[Loading] \tCreating " + t);
				AddGlobalObject(Activator.CreateInstance(t), t);
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
			var sceneMgr = GetG<SceneManager>();

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
			return new Root("plugins.cfg", "", "Ponykart.log");
		}

		private static RenderSystem InitRenderSystem(Root root) {
			Launch.Log("[Loading] Creating RenderSystem...");

			if (root.RenderSystem == null) {
				var renderSystem = root.GetRenderSystemByName("Direct3D9 Rendering Subsystem");
				renderSystem.SetConfigOption("Full Screen", "No");
				renderSystem.SetConfigOption("Video Mode", Settings.Default.WindowWidth + " x " + Settings.Default.WindowHeight + " @ 32-bit colour");

				root.RenderSystem = renderSystem;
			}

			return root.RenderSystem;
		}

		/// <summary>
		/// Creates the render window, tells it to use our WinForms window, and enables vsync
		/// </summary>
		private static RenderWindow InitRenderWindow(Root root, Form form, RenderSystem renderSystem) {
			Launch.Log("[Loading] Creating RenderWindow...");

			//Ensure RenderSystem has been Initialised
			root.RenderSystem = renderSystem;

			root.Initialise(false, "Ponykart");
			NameValuePairList miscParams = new NameValuePairList();

			if (form.Handle != IntPtr.Zero)
				miscParams["externalWindowHandle"] = form.Handle.ToString();

			miscParams["vsync"] = "true";    // by Ogre default: false

			return root.CreateRenderWindow("Ponykart main RenderWindow", Settings.Default.WindowWidth, Settings.Default.WindowHeight, false, miscParams);
		}

		/// <summary>
		/// Creates the scene manager and sets up shadow stuff
		/// </summary>
		private static SceneManager InitSceneManager(Root root) {
			Launch.Log("[Loading] Creating SceneManager");
			var sceneMgr = root.CreateSceneManager("OctreeSceneManager", "sceneMgr");
			sceneMgr.ShadowColour = new ColourValue(0.8f, 0.8f, 0.8f);
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;
			return sceneMgr;
		}

		private static Viewport InitViewport(RenderWindow window, Camera camera) {
			Launch.Log("[Loading] Creating Viewport...");
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
