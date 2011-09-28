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
		private static readonly string _ret = "\r\n", _comma = " , ";
		bool FrameStarted(FrameEvent evt) {
			if (elapsed >= 0.1f) {
				elapsed = 0;

				var mainPlayer = LKernel.GetG<PlayerManager>().MainPlayer;

				if (LKernel.GetG<LevelManager>().IsValidLevel && mainPlayer != null && mainPlayer.Kart != null && !mainPlayer.Kart.Body.IsDisposed) {
					Kart kart = mainPlayer.Kart;
					
					label.Text =
						string.Concat("Speed: ", kart.Vehicle.CurrentSpeedKmHour, _ret,
						"Turn angle: ", Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(0)), _comma, Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(1)), _comma,
						Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(2)), _comma, Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(3)), _ret, 
						"Linear Velocity: ", kart.Body.LinearVelocity.Length, _comma, kart.Body.LinearVelocity, _ret,
						"WheelFriction: ", kart.Vehicle.GetWheelInfo(0).FrictionSlip, _comma, kart.Vehicle.GetWheelInfo(2).FrictionSlip, _ret,
						"SkidInfo: ", kart.Vehicle.GetWheelInfo(0).SkidInfo, _comma, kart.Vehicle.GetWheelInfo(2).SkidInfo, _ret,
						"Brake? ", kart.WheelFL.IsBrakeOn, _ret, 
						"AccelMultiplier: ", kart.Acceleration, _ret, 
						"Gravity: ", kart.Body.Gravity, _ret,
						"Bouncing: ", kart.IsBouncing, " , Drifting: ", kart.IsDrifting, _comma, kart.WheelFL.DriftState);
				}
			}
			elapsed += evt.timeSinceLastFrame;
			return true;
		}
	}
}
#endif