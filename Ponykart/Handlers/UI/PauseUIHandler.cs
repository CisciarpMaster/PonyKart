using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Ponykart.Core;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class hooks up to whenever the game is paused and displays the pause UI.
	/// At the moment it's just a big label that says "PAUSED" but hey it's a start.
	/// Later we can expand this to bring up a main menu or something.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class PauseUIHandler {

		private Label pauseLabel;

		public PauseUIHandler() {
			// create our label
			var gui = LKernel.Get<UIMain>().Gui;

			pauseLabel = new Label("pause label") {
				Location = new Point(0, 0), 
				Size = new Size((int) Constants.WINDOW_WIDTH, (int) Constants.WINDOW_HEIGHT),
				Visible = false,
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Coral,
					Font = UIResources.Fonts["BlueHighwayHuge"],
				},
				Text = "PAUSED",
				AlwaysOnTop = true,
			};
			gui.Controls.Add(pauseLabel);

			// hook up to the pause event
			LKernel.Get<Pauser>().PauseEvent += new PauseEventHandler(DoOnPause);
		}

		void DoOnPause(bool isPaused) {
			pauseLabel.Visible = isPaused;
		}
	}
}
