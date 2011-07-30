using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Mogre;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles the progress bar and label that show on loading screens
	/// </summary>
	public class LoadingUIHandler {
		Label label;

		public LoadingUIHandler() {
			Launch.Log("[Loading] Creating LoadingUIHandler");
			LKernel.Get<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
			LKernel.Get<LevelManager>().OnLevelUnload += new LevelEventHandler(OnLevelUnload);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (label != null)
				label.Dispose();
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			label = new Label("loading label") {
				Size = new Size((int) Constants.WINDOW_WIDTH, (int) Constants.WINDOW_HEIGHT),
				Location = new Point(0, 0),
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Coral,
					Font = UIResources.Fonts["BlueHighwayHuge"],
				},
				Text = "Loading...",
			};

			var gui = LKernel.Get<UIMain>().Gui;
			gui.Controls.Add(label);
			LKernel.Get<UIMain>().MiyagiSys.Update();
			LKernel.Get<Root>().RenderOneFrame();
		}
	}
}
