using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mogre;
using Ponykart.Properties;

namespace Ponykart {
	public static partial class LKernel {

		/// <summary>
		/// Initialises the root
		/// </summary>
		private static Root InitRoot() {
			return new Root("plugins.cfg", "", "Ponykart.log");
		}

		/// <summary>
		/// Initialises the render system, tells it to use directx, windowed, etc
		/// </summary>
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

			// this lets us use a winforms window instead of one from ogre
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
			return sceneMgr;
		}

		/// <summary>
		/// Creates the viewport
		/// </summary>
		private static Viewport InitViewport(RenderWindow window, Camera camera) {
			Launch.Log("[Loading] Creating Viewport...");
			return window.AddViewport(camera);
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
		/// This is where resources are actually loaded into memory.
		/// </summary>
		private static void LoadResourceGroups() {
			TextureManager.Singleton.DefaultNumMipmaps = 1;

#if DEBUG
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();
#else
			ResourceGroupManager.Singleton.InitialiseResourceGroup("Bootstrap");
			ResourceGroupManager.Singleton.InitialiseResourceGroup("General");
#endif
		}
	}
}
