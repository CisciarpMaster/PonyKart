using System;
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
		public void Go() {
			this.InitializeComponent();
			this.InitializeOgre();
			base.Show();
			this.StartRendering();
		}

		/// <summary>
		/// Sets up the Form window
		/// </summary>
		private void InitializeComponent() {
			this.SuspendLayout();
			// 
			// Main
			// 
			this.ClientSize = new System.Drawing.Size(1008, 730);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Ponykart";
			this.ResumeLayout(false);

			base.Icon = Ponykart.Properties.Resources.Icon_1;
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

		// ====================================================================
		
		/// <summary>
		/// Gets the handle pointer for this window. Needed by RenderWindowProvider.
		/// </summary>
		new public IntPtr Handle {
			get { return base.Handle; }
		}
	}
}
