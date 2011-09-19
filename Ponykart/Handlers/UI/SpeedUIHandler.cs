using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.UI.Controls;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.UI;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Global)]
	public class SpeedUIHandler {
		Label label;

		public SpeedUIHandler() {
			var gui = LKernel.GetG<UIMain>().Gui;

			label = new Label("speed label") {
				Location = new Point(10, 400),
				Size = new Size(500, 300),
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

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed >= 0.1f) {
				elapsed = 0;

				var mainPlayer = LKernel.GetG<PlayerManager>().MainPlayer;

				if (LKernel.GetG<LevelManager>().IsValidLevel && mainPlayer != null && mainPlayer.Kart != null && !mainPlayer.Kart.Body.IsDisposed) {
					Kart kart = mainPlayer.Kart;
					label.Text =
						"Speed: " + kart.Vehicle.CurrentSpeedKmHour + "\r\n" +
						"Turn angle: " + kart.Vehicle.GetSteeringValue((int) WheelID.FrontLeft) + "\r\n" +
						"Linear Velocity: " + kart.Body.LinearVelocity.Length + "  " + kart.Body.LinearVelocity + "\r\n" +
						"WheelFriction: " + kart.Vehicle.GetWheelInfo(0).FrictionSlip + " , " + kart.Vehicle.GetWheelInfo(2).FrictionSlip + "\r\n" +
						"Brake? " + kart.WheelFL.IsBrakeOn + "\r\n" +
						"AccelMultiplier: " + kart.WheelFL.AccelerateMultiplier + "\r\n" +
						"Gravity: " + kart.Body.Gravity;
				}
			}
			elapsed += evt.timeSinceLastFrame;
			return true;
		}
	}
}
