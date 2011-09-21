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
		Panel panel;

		public LoadingUIHandler() {
			LKernel.GetG<LevelManager>().OnLevelPostLoad += OnLevelPostLoad;
			LKernel.GetG<LevelManager>().OnLevelPreUnload += OnLevelPreUnload;

			panel = new Panel("loading screen panel") {
				Size = new Size((int) Settings.Default.WindowWidth, (int) Settings.Default.WindowHeight),
				Location = new Point(0, 0),
				Visible = false,
				AlwaysOnTop = true,
				Skin = UIResources.Skins["PKLoadingScreenPanel"],
			};

			LKernel.GetG<UIMain>().Gui.Controls.Add(panel);
		}

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			panel.Visible = false;
		}

		void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			panel.Visible = true;
		}
	}
}
