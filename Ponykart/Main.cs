using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mogre;
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
			this.ClientSize = new Size(1008, 730);
			this.FormBorderStyle = FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Ponykart";
			this.ResumeLayout(false);

			base.Icon = new Icon(Settings.Default.Icon);
		}

		/// <summary>
		/// Sets up ogre, unsurprisingly
		/// </summary>
		private void InitializeOgre() {
			Splash splash = new Splash(12);
			splash.Show();
			try
			{
				LKernel.LoadInitialObjects(splash);
				base.Disposed += (sender, ea) => {
					LKernel.Get<UIMain>().Dispose();
					if (LKernel.Get<Root>() != null)
						LKernel.Get<Root>().Shutdown();
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
			Root root = LKernel.Get<Root>();
			root.RenderOneFrame();
			
			while (!quit && !this.IsDisposed && root != null) {
				if (!root.RenderOneFrame())
					break;
				Application.DoEvents();
			}
			LKernel.Get<UIMain>().Dispose();
			if (root != null)
				root.Shutdown();
		}

		// ====================================================================

		protected override void Dispose(bool disposing) {
			if (LKernel.Get<UIMain>() != null)
				LKernel.Get<UIMain>().Dispose();
			base.Dispose(disposing);
		}
	}
}
