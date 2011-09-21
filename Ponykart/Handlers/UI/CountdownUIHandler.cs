using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.UI;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Global)]
	public class CountdownUIHandler {
		private Label countLabel;

		public CountdownUIHandler() {
			// set up our label
			GUI Gui = LKernel.GetG<UIMain>().Gui;

			countLabel = new Label("countdown label") {
				Location = new Point(0, 0),
				Size = new Size((int) Settings.Default.WindowWidth, (int) Settings.Default.WindowHeight),
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.Magenta,
					Font = UIResources.Fonts["CelestiaHuge"],
				},
				Visible = false,
				Text = string.Empty,
			};

			Gui.Controls.Add(countLabel);

			// hook up to events
			LKernel.GetG<RaceCountdown>().OnCountdown += new RaceCountdownEvent(OnCountdown);
			LKernel.GetG<LevelManager>().OnLevelPreUnload += new LevelEvent(OnLevelPreUnload);
		}

		void OnLevelPreUnload(LevelChangedEventArgs eventArgs) {
			countLabel.Visible = false;
		}


		void OnCountdown(RaceCountdownState state) {
			switch (state) {
				case RaceCountdownState.Three:
					countLabel.Visible = true;
					countLabel.Text = "3";
					break;
				case RaceCountdownState.Two:
					countLabel.Text = "2";
					break;
				case RaceCountdownState.One:
					countLabel.Text = "1";
					break;
				case RaceCountdownState.Go:
					countLabel.Text = "Go!";
					break;
				case RaceCountdownState.OneSecondAfterGo:
					countLabel.Visible = false;
					break;
			}
		}
	}
}
