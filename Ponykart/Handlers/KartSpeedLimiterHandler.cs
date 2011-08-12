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
	[Handler(HandlerScope.Level)]
	public class KartSpeedLimiterHandler : ILevelHandler {
		PlayerManager playerManager;

		public KartSpeedLimiterHandler() {
			playerManager = LKernel.Get<PlayerManager>();
			LKernel.Get<PhysicsMain>().PostSimulate += PostSimulate;
		}

		/// <summary>
		/// Runs after every physics simulation
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!LKernel.Get<LevelManager>().IsValidLevel)
				return;

			// every kart
			foreach (Player p in playerManager.Players) {
				// make sure the karts are valid
				if (p == null || p.Kart == null || p.Body.IsDisposed) {
					Launch.Log("[WARNING] (KartSpeedLimiterHandler) A player/kart was found that was null!");
					continue;
				}

				Kart kart = p.Kart;
				// check its velocity against the max velocity (both are squared to avoid unnecessary square roots)
				if (kart.Body.LinearVelocity.SquaredLength > kart.MaxSpeedSquared) {
					Vector3 vec = kart.Body.LinearVelocity;
					vec.Normalise();
					vec *= kart.MaxSpeed;
					kart.Body.LinearVelocity = vec;
				}
			}
		}

		public void Dispose() {
			LKernel.Get<PhysicsMain>().PostSimulate -= PostSimulate;
		}
	}
}
