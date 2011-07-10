using Mogre;

namespace Lymph.Stuff {
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
			LKernel.Get<Root>().FrameEnded += FrameEnded;

			ShowDebugOverlay(true);
		}

		/// <summary>
		/// Copypasta'd. Updates DebugOverlay. Called every frame.
		/// </summary>
		protected void UpdateStats() {
			string currFps = "Current FPS: ";
			string avgFps = "Average FPS: ";
			string bestFps = "Best FPS: ";
			string worstFps = "Worst FPS: ";
			string tris = "Triangle Count: ";
			string batches = "Batch Count: ";

			// update stats when necessary
			OverlayElement guiAvg = OverlayManager.Singleton.GetOverlayElement("Core/AverageFps", false);
			OverlayElement guiCurr = OverlayManager.Singleton.GetOverlayElement("Core/CurrFps", false);
			OverlayElement guiBest = OverlayManager.Singleton.GetOverlayElement("Core/BestFps", false);
			OverlayElement guiWorst = OverlayManager.Singleton.GetOverlayElement("Core/WorstFps", false);
			OverlayElement guiTris = OverlayManager.Singleton.GetOverlayElement("Core/NumTris", false);
			OverlayElement guiBatches = OverlayManager.Singleton.GetOverlayElement("Core/NumBatches", false);
			OverlayElement guiDbg = OverlayManager.Singleton.GetOverlayElement("Core/DebugText", false);

			RenderTarget.FrameStats stats = LKernel.Get<RenderWindow>().GetStatistics();

			guiAvg.Caption = avgFps + stats.AvgFPS;
			guiCurr.Caption = currFps + stats.LastFPS;
			guiBest.Caption = bestFps + stats.BestFPS + " " + stats.BestFrameTime + " ms";
			guiWorst.Caption = worstFps + stats.WorstFPS + " " + stats.WorstFrameTime + " ms";
			guiTris.Caption = tris + stats.TriangleCount;
			guiBatches.Caption = batches + stats.BatchCount;
			guiDbg.Caption = debugText;
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

		public bool FrameEnded(FrameEvent e) {
			UpdateStats();
			return true;
		}
	}
}
