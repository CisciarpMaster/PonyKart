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
	[Handler(HandlerScope.Global)]
	public class LoadingUIHandler {
		Label label;

		public LoadingUIHandler() {
			LKernel.Get<LevelManager>().OnLevelLoad += OnLevelLoad;
			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;

			label = new Label("loading label") {
				Size = new Size((int) Constants.WINDOW_WIDTH, (int) Constants.WINDOW_HEIGHT),
				Location = new Point(0, 0),
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Coral,
					Font = UIResources.Fonts["BlueHighwayHuge"],
				},
				Text = "Loading...",
				Visible = false,
			};
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			label.Visible = false;
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			label.Visible = true;
			LKernel.Get<UIMain>().MiyagiSys.Update();
			LKernel.Get<Root>().RenderOneFrame();
		}
	}
}
