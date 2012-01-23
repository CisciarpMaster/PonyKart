using System.Collections.Generic;
using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Core {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DriverAnimationHandler : ILevelHandler {
		List<Kart> karts;

		public DriverAnimationHandler() {
			karts = new List<Kart>();

			foreach (var p in LKernel.GetG<PlayerManager>().Players) {
				karts.Add(p.Kart);
			}

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		float elapsed = 0;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed > 0.1f) {
				foreach (var kart in karts) {
					if (kart.VehicleSpeed >= -30 || kart.IsInAir) {
						// forwards
						if (kart.DriftState == KartDriftState.StartLeft || kart.DriftState == KartDriftState.FullLeft) {
							kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.DriftLeft);
						}
						else if (kart.DriftState == KartDriftState.StartRight || kart.DriftState == KartDriftState.FullRight) {
							kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.DriftRight);
						}
						else {
							if (kart.TurnMultiplier == 0) {
								// straight
								kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.Drive);
							}
							else if (kart.TurnMultiplier < 0) {
								// right
								kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.TurnRight);
							}
							else {
								// left
								kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.TurnLeft);
							}
						}
					}
					else {
						// reverse
						// don't have multiple reverse anims at the moment
						kart.Driver.ChangeAnimationIfNotBlending(DriverAnimation.Reverse, AnimationBlendingTransition.BlendWhileAnimating, 0.3f);
					}
				}

				elapsed = 0;
			}

			elapsed += evt.timeSinceLastFrame;
			return true;
		}

		public void Detach() {
			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
		}
	}
}
