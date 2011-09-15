using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;

namespace Ponykart.Handlers {
	/// <summary>
	/// A little handler to limit the speed of karts.
	/// </summary>
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class KartSpeedLimiterHandler : ILevelHandler {
		PlayerManager playerManager;

		public KartSpeedLimiterHandler() {
			playerManager = LKernel.GetG<PlayerManager>();
			LKernel.GetG<PhysicsMain>().PreSimulate += PreSimulate;
		}

		/// <summary>
		/// Runs after every physics simulation
		/// </summary>
		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!LKernel.GetG<LevelManager>().IsValidLevel)
				return;

			// every kart
			foreach (Player player in playerManager.Players) {
				// make sure the karts are valid
				if (player == null || player.Kart == null || player.Body.IsDisposed) {
					Launch.Log("[WARNING] (KartSpeedLimiterHandler) A player/kart was found that was null!");
					continue;
				}

				Kart kart = player.Kart;

				// going forwards
				// using 20 because we don't need to check the kart's linear velocity if it's going really slowly
				if (kart.Vehicle.CurrentSpeedKmHour > 20 && !kart.IsInAir) {
					// check its velocity against the max velocity (both are squared to avoid unnecessary square roots)
					if (kart.Body.LinearVelocity.SquaredLength > kart.MaxSpeedSquared) {
						Vector3 vec = kart.Body.LinearVelocity;
						vec.Normalise();
						vec *= kart.MaxSpeed;
						kart.Body.LinearVelocity = vec;
					}
				}
				// going in reverse, so we want to limit the speed even more
				else if (kart.Vehicle.CurrentSpeedKmHour < -20 && !kart.IsInAir) {
					if (kart.Body.LinearVelocity.SquaredLength > kart.MaxReverseSpeedSquared) {
						Vector3 vec = kart.Body.LinearVelocity;
						vec.Normalise();
						vec *= kart.MaxReverseSpeed;
						kart.Body.LinearVelocity = vec;
					}
				}
			}
		}

		public void Detach() {
			LKernel.GetG<PhysicsMain>().PreSimulate -= PreSimulate;
		}
	}
}
