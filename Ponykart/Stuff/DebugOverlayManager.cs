using Mogre;

namespace Ponykart.Stuff {
	public class DebugOverlayManager {
		/// <summary>
		/// The Ogre DebugOverlay.
		/// </summary>
		private Overlay overlay;
		/// <summary>
		/// String to be displayed in the middle of the DebugOverlay.
		/// </summary>
		private string debugText = "";

		public DebugOverlayManager() {
			this.overlay = OverlayManager.Singleton.GetByName("Core/DebugOverlay");
			LKernel.GetG<Root>().FrameStarted += FrameStarted;

#if DEBUG
			ShowDebugOverlay(true);
#endif
		}

		/// <summary>
		/// Copypasta'd. Updates DebugOverlay. Called every frame.
		/// </summary>
		protected void UpdateStats() {
			string currFps = "Current FPS: ";
			string avgFps = "Average FPS: ";
			string tris = "Triangle Count: ";
			string batches = "Batch Count: ";

			// update stats when necessary
			OverlayElement guiAvg = OverlayManager.Singleton.GetOverlayElement("Core/AverageFps", false);
			OverlayElement guiCurr = OverlayManager.Singleton.GetOverlayElement("Core/CurrFps", false);
			OverlayElement guiTris = OverlayManager.Singleton.GetOverlayElement("Core/NumTris", false);
			OverlayElement guiBatches = OverlayManager.Singleton.GetOverlayElement("Core/NumBatches", false);

			RenderTarget.FrameStats stats = LKernel.GetG<RenderWindow>().GetStatistics();

			guiAvg.Caption = avgFps + stats.AvgFPS;
			guiCurr.Caption = currFps + stats.LastFPS;
			guiTris.Caption = tris + stats.TriangleCount;
			guiBatches.Caption = batches + stats.BatchCount;
		}

		/// <summary> 
		/// Turns debug overlay on or off
		/// </summary>
		public void ShowDebugOverlay(bool show) {
			if (this.overlay != null) {
				if (show)
					overlay.Show();
				else
					overlay.Hide();
			}
		}

		public void ToggleDebugOverlay() {
			ShowDebugOverlay(!overlay.IsVisible);
		}

		public bool FrameStarted(FrameEvent e) {
			UpdateStats();
			return true;
		}
	}
}
