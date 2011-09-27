#if DEBUG
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
			var gui = LKernel.GetG<UIMain>().GetGUI("level gui");

			label = gui.GetControl<Label>("speed label");

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);
		}

		float elapsed;
		private static readonly string _ret = "\r\n";
		bool FrameStarted(FrameEvent evt) {
			if (elapsed >= 0.1f) {
				elapsed = 0;

				var mainPlayer = LKernel.GetG<PlayerManager>().MainPlayer;

				if (LKernel.GetG<LevelManager>().IsValidLevel && mainPlayer != null && mainPlayer.Kart != null && !mainPlayer.Kart.Body.IsDisposed) {
					Kart kart = mainPlayer.Kart;
					
					label.Text =
						string.Concat("Speed: ", kart.Vehicle.CurrentSpeedKmHour, _ret, 
						"Turn angle: ", kart.Vehicle.GetSteeringValue((int) WheelID.FrontLeft), _ret, 
						"Linear Velocity: ", kart.Body.LinearVelocity.Length, "  ", kart.Body.LinearVelocity, _ret, 
						"WheelFriction: ", kart.GetWheel(0).FrictionSlip, " , ", kart.GetWheel(2).FrictionSlip, _ret, 
						"Brake? ", kart.WheelFL.IsBrakeOn, _ret, 
						"AccelMultiplier: ", kart.Acceleration, _ret, 
						"Gravity: ", kart.Body.Gravity, _ret,
						"Bouncing: ", kart.IsBouncing, " , Drifting: ", kart.IsDrifting);
				}
			}
			elapsed += evt.timeSinceLastFrame;
			return true;
		}
	}
}
#endif