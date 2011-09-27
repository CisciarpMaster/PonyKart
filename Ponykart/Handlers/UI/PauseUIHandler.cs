using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Ponykart.Core;
using Ponykart.Properties;
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
			var gui = LKernel.GetG<UIMain>().Gui;

			pauseLabel = new Label("pause label") {
				Location = new Point(0, 0),
				Size = new Size((int) Settings.Default.WindowWidth, (int) Settings.Default.WindowHeight),
				Visible = false,
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Red,
					Font = UIResources.Fonts["Celestia"],
				},
				Text = "PAUSED",
			};
			gui.Controls.Add(pauseLabel);

			// hook up to the pause event
			LKernel.GetG<Pauser>().PauseEvent += new PauseEvent(DoOnPause);
		}

		void DoOnPause(PausingState state) {
			if (state == PausingState.Pausing)
				pauseLabel.Visible = true;
			else
				pauseLabel.Visible = false;
		}
	}
}
