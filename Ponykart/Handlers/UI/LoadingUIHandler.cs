using Miyagi.UI;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles the progress bar and label that show on loading screens
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LoadingUIHandler {
		GUI loadingGui;

		public LoadingUIHandler() {
			LKernel.GetG<LevelManager>().OnLevelPostLoad += OnLevelPostLoad;
			LKernel.GetG<LevelManager>().OnLevelPreUnload += OnLevelPreUnload;

			loadingGui = LKernel.GetG<UIMain>().GetGUI("loading screen gui");
		}

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			loadingGui.Visible = false;
		}

		void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			loadingGui.Visible = true;
		}
	}
}
