using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mogre;
using Ponykart.Properties;
using Ponykart.UI;

namespace Ponykart {
	/// <summary>
	/// The startup class/window.
	/// </summary>
	public class Main : Form {
		public static bool quit = false;
		/// <summary>
		/// Should only be run once, right when you start up. This is what Launch calls.
		/// </summary>
		[DebuggerStepThrough]
		public void Go() {
			InitializeComponent();
			InitializeOgre();
			base.Show();
			StartRendering();
		}

		/// <summary>
		/// Sets up the Form window
		/// </summary>
		private void InitializeComponent() {
			this.SuspendLayout();
			// 
			// Main
			// 
			this.ClientSize = new Size((int) Settings.Default.WindowWidth, (int) Settings.Default.WindowHeight);
			this.FormBorderStyle = FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Ponykart";
			this.ResumeLayout(false);

			base.Icon = Resources.Icon_2;
		}

		/// <summary>
		/// Sets up ogre, unsurprisingly
		/// </summary>
		private void InitializeOgre() {
			Splash splash = new Splash();
			splash.Show();
			try {
				LKernel.LoadInitialObjects(splash);
				base.Disposed += (sender, ea) => {
					LKernel.GetG<UIMain>().Dispose();
					if (LKernel.GetG<Root>() != null)
						LKernel.GetG<Root>().Shutdown();
				};
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
		private void StartRendering() {
			Root root = LKernel.GetG<Root>();
			root.RenderOneFrame();

			while (!quit && !this.IsDisposed && root != null) {
				if (!root.RenderOneFrame())
					break;
				Application.DoEvents();
			}
			LKernel.GetG<UIMain>().Dispose();
			if (root != null)
				root.Shutdown();
		}

		// ====================================================================

		protected override void Dispose(bool disposing) {
			if (LKernel.GetG<UIMain>() != null)
				LKernel.GetG<UIMain>().Dispose();
			base.Dispose(disposing);
		}
	}
}
