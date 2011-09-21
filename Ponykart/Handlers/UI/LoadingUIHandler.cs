using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles the progress bar and label that show on loading screens
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LoadingUIHandler {
		Label label;

		public LoadingUIHandler() {
			LKernel.GetG<LevelManager>().OnLevelPostLoad += OnLevelPostLoad;
			LKernel.GetG<LevelManager>().OnLevelPreUnload += OnLevelPreUnload;

			label = new Label("loading label") {
				Size = new Size((int) Settings.Default.WindowWidth, (int) Settings.Default.WindowHeight),
				Location = new Point(0, 0),
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Red,
					Font = UIResources.Fonts["BlueHighwayHuge"],
				},
				Text = "Loading...",
				Visible = false,
				AlwaysOnTop = true,
			};

			LKernel.GetG<UIMain>().Gui.Controls.Add(label);
		}

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			label.Visible = false;
		}

		void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			label.Visible = true;
		}
	}
}
