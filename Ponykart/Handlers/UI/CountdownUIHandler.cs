using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Core;
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
					Font = UIResources.Fonts["BlueHighwayHuge"],
				},
				Visible = false,
				Text = string.Empty,
				AlwaysOnTop = true,
			};

			Gui.Controls.Add(countLabel);

			// hook up to events
			var countdown = LKernel.GetG<RaceCountdown>();
			countdown.OnThree += new RaceCountEvent(OnThree);
			countdown.OnTwo += new RaceCountEvent(OnTwo);
			countdown.OnOne += new RaceCountEvent(OnOne);
			countdown.OnGo += new RaceCountEvent(OnGo);
			countdown.OnOneSecondAfterGo += new RaceCountEvent(OnOneSecondAfterGo);
		}


		void OnThree() {
			countLabel.Visible = true;
			countLabel.Text = "3";
		}

		void OnTwo() {
			countLabel.Text = "2";
		}

		void OnOne() {
			countLabel.Text = "1";
		}

		void OnGo() {
			countLabel.Text = "Go!";
		}

		void OnOneSecondAfterGo() {
			countLabel.Visible = false;
		}
	}
}
