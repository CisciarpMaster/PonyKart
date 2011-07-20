using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Ponykart {
	/// <summary>
	/// This class manages the splash screen you see when you start up the game.
	/// </summary>
	public class Splash : Form {
		private IContainer components;
		private Label LoadingText;
		private ProgressBar Progress;
		private int maximum;

		public readonly Bitmap Picture = Properties.Resources.LymphSplash;

		/// <summary>
		/// Sets up the splash screen
		/// </summary>
		/// <param name="numberOfIncrements">
		/// The total calls to .Increment() you are going to do on the splash screen.
		/// Added to the constructor for convenience.
		/// </param>
		public Splash(int numberOfIncrements) {
			this.maximum = numberOfIncrements;
			this.InitializeComponent();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
		}

		protected override void Dispose(bool disposing) {
			if (disposing && (this.components != null)) {
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Adds 1 to the progress bar value and changes the loading text to the supplied argument.
		/// </summary>
		/// <param name="text">The text that you see below the progress bar</param>
		public void Increment(string text) {
			if (Progress.Value < Progress.Maximum)
				this.Progress.Increment(1);
			this.LoadingText.Text = text;
			base.Update();
			Launch.Log("[Loading] " + text);
			Application.DoEvents();
		}

		/// <summary>
		/// Set up the window
		/// </summary>
		private void InitializeComponent() {
			components = new Container();
			this.LoadingText = new Label();
			components.Add(LoadingText);
			this.Progress = new ProgressBar();
			components.Add(Progress);
			this.SuspendLayout();
			// 
			// LoadingText
			// 
			this.LoadingText.AutoEllipsis = true;
			this.LoadingText.BackColor = Color.Transparent;
			this.LoadingText.Dock = DockStyle.Bottom;
			this.LoadingText.Location = new Point(0, Picture.Height + 22);
			this.LoadingText.Name = "LoadingText";
			this.LoadingText.Size = new Size(Picture.Width - 1, 20);
			this.LoadingText.TabIndex = 0;
			this.LoadingText.Text = "Loading...";
			this.LoadingText.UseWaitCursor = true;
			// 
			// Progress
			// 
			this.Progress.Location = new Point(0, Picture.Height - 1);
			this.Progress.Maximum = this.maximum;
			this.Progress.Name = "Progress";
			this.Progress.Size = new Size(Picture.Width - 1, 23);
			this.Progress.Step = 1;
			this.Progress.TabIndex = 1;
			this.Progress.UseWaitCursor = true;
			// 
			// Splash
			// 
			this.BackColor = SystemColors.Window;
			this.BackgroundImage = Picture;
			this.BackgroundImageLayout = ImageLayout.None;
			this.ClientSize = new Size(Picture.Width, Picture.Height + 42);
			this.Controls.Add(this.Progress);
			this.Controls.Add(this.LoadingText);
			this.DoubleBuffered = true;
			this.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
			this.FormBorderStyle = FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.Name = "Splash";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Loading Lymph...";
			this.UseWaitCursor = true;
			this.ResumeLayout(false);

			base.Icon = Properties.Resources.Icon_1;
		}

		new public void Show() {
			base.Show();
			Application.DoEvents();
		}
	}
}