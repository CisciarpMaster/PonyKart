using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.UI;

namespace Ponykart.Handlers {
	public class SpeedUIHandler {
		Label label;

		public SpeedUIHandler() {
			var gui = LKernel.Get<UIMain>().Gui;

			label = new Label("speed label") {
				Location = new Point(10, 400),
				Size = new Size(200, 200),
				Visible = true,
				TextStyle = {
					Alignment = Alignment.TopLeft,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["BlueHighway"],
				},
				Text = "Speed",
				AlwaysOnTop = true,
			};
			gui.Controls.Add(label);

			LKernel.Get<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed >= 0.2f) {
				elapsed = 0;

				var mainPlayer = LKernel.Get<PlayerManager>().MainPlayer;

				if (LKernel.Get<LevelManager>().IsValidLevel && mainPlayer != null && mainPlayer.Kart != null && !mainPlayer.Kart.Actor.IsDisposed) {
					Kart kart = mainPlayer.Kart;
					label.Text =
						"    Speed    Angle\r\n" +
						"FR: " + kart.WheelFR.Shape.AxleSpeed + " " + Math.RadiansToDegrees(kart.WheelFR.Shape.SteerAngle) + "\r\n" +
						"FL: " + kart.WheelFL.Shape.AxleSpeed + " " + Math.RadiansToDegrees(kart.WheelFL.Shape.SteerAngle) + "\r\n" +
						"BR: " + kart.WheelBR.Shape.AxleSpeed + "\r\n" +
						"BL: " + kart.WheelBL.Shape.AxleSpeed + "\r\n" +
						"Linear Velocity: " + kart.Actor.LinearVelocity.Length;
				}
			}
			elapsed += evt.timeSinceLastFrame;
			return true;
		}
	}
}
