using System;
using System.Diagnostics;
using System.Windows.Forms;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;
using Ponykart.UI;

namespace Ponykart {
	public static class Launch {

		public static bool Quit = false;

		[STAThread]
		public static void Main() {
			//#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
			//#endif

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");

			LKernel.Initialise();
			Options.Initialise();

			InitializeOgre();
			StartRendering();
		}

		private static void InitializeOgre() {
			Splash splash = new Splash();
			splash.Show();
			try {
				LKernel.LoadInitialObjects(splash);
				GC.Collect();
			}
			finally {
				splash.Close();
				splash.Dispose();
			}
		}

		/// <summary>
		/// Starts the render loop!
		/// </summary>
		[DebuggerStepThrough]
		private static void StartRendering() {
			Root root = LKernel.GetG<Root>();
			RenderWindow window = LKernel.GetG<RenderWindow>();

			root.RenderOneFrame();
			window.SetVisible(true);

			while (!Quit && !window.IsClosed && root != null) {
				if (!root.RenderOneFrame())
					break;
				// this is for stuff like window selection, moving, etc
				Application.DoEvents();
			}

			LKernel.GetG<UIMain>().Dispose();
			LKernel.GetG<PhysicsMain>().Dispose();
			if (root != null)
				root.Shutdown();
		}

		/// <summary>
		/// Fired whan an unhandled exception bubbles up to the AppDomain
		/// </summary>
		static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			// message box
			if (OgreException.IsThrown)
				MessageBox.Show(OgreException.LastException.FullDescription, "An Ogre exception has occurred!");
			else {
				var ex = e.ExceptionObject as Exception;
				if (ex != null)
					MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, ex.GetType().ToString());
			}
		}

		/// <summary>
		/// Writes something to the console and also sticks it in ogre's log file
		/// </summary>
		/// <param name="message">The message to log</param>
		[DebuggerHidden]
		public static void Log(string message) {
			Debug.WriteLine(message);
			if (LogManager.Singleton != null)
				LogManager.Singleton.LogMessage(message);
		}
	}
}
