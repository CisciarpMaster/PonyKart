using System.Drawing;
using Miyagi.Common;
using Miyagi.Common.Resources;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Mogre;
using Ponykart.Levels;

namespace Ponykart.UI {
	/// <summary>
	/// The class that manages the UI system
	/// </summary>
	public class UIMain : LDisposable {

		/// <summary>
		/// The Miyagi system - this pretty much controls everything in the GUI or has pointers to classes that do so
		/// </summary>
		public MiyagiSystem MiyagiSys { get; private set; }
		/// <summary>
		/// Our current GUI layout
		/// </summary>
		public GUIManager GuiManager {
			get {
				return MiyagiSys.GUIManager;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public UIMain() {
			Launch.Log("[Loading] Starting Miyagi...");
			var levelManager = LKernel.GetG<LevelManager>();
			var input = LKernel.GetG<InputMain>();

			// attach events
			LKernel.GetG<Root>().FrameStarted += FrameStarted;

			// set up the system - the first argument has to be "Mogre" because that's the system we're running Miyagi on
			MiyagiSys = new MiyagiSystem("Mogre");
			// load the MOIS plugin
			MiyagiSys.PluginManager.LoadPlugin("Miyagi.Plugin.Input.Mois.dll", input.InputKeyboard, input.InputMouse);

			// load the resources
			UIResources.Create(MiyagiSys);
			UIResources.CreateCursor(MiyagiSys.GUIManager);

			// add a default GUI to the GUIManager
			var gui = new GUI("default GUI");
			MiyagiSys.GUIManager.GUIs.Add(gui);

			Launch.Log("[Loading] Miyagi loaded!");
		}

		/// <summary>
		/// Runs on each frame
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			//var mgr = MiyagiSys.TwoDManager;
			//if (mgr.GetElement("FPS") != null) {
				//mgr.GetElement<Miyagi.TwoD.Layers.TextOverlay>("FPS").Text = "FPS: " + LKernel.Get<RenderWindow>().LastFPS;
				// guiMgr.GetControl<Miyagi.UI.Controls.Label>("Batch").Text = "Batch: " + root.RenderSystem._getBatchCount();
				// guiMgr.GetControl<Miyagi.UI.Controls.Label>("Vertex").Text = "Vertex: " + root.RenderSystem._getVertexCount();
			//}

			if (MiyagiSys != null && !MiyagiSys.IsDisposed)
				MiyagiSys.Update();
			return true;
		}

		public GUI GetGUI(string name) {
			return MiyagiSys.GUIManager.GetGUI(name);
		}

		public void Serialize() {
			MiyagiSys.SerializationManager.ExportToFile("media/gui/serialize.mgx");
		}

		public void ExportImageFont(string ttfName, FontStyle style = FontStyle.Regular, int size = 12, int resolution = 96) {
			TrueTypeFont.TrueTypeToImageFont("media/gui/Fonts/", "media/gui/Fonts/" + ttfName, style, size, resolution);
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			// TODO: figure out how to stop miyagi from throwing errors whenever we shut down the program
			MiyagiSys.Dispose();

			base.Dispose(disposing);
		}


		/// <summary>
		/// Given a point on the screen, is the point "obstructed" by a GUI element?
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>True if it is obstructed, false if it is not</returns>
		public bool IsPointObstructed(int x, int y) {
			// get the top control at the point
			Control control = MiyagiSys.GUIManager.GetTopControlAt(x, y);
			// is there a control there?
			if (control == null)
				// if not, then it's not obstructed, so return
				return false;

			// otherwise look in the user data to see if this control will obstruct
			UIUserData ud = control.UserData as UIUserData;
			if (ud != null)
				return ud.ObstructsViewport;
			else
				// does not have any user data, so we just go with the default
				return false;
		}
	}
}
