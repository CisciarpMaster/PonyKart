using BulletSharp;
using Ponykart.Actors;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DriftingHandler : ILevelHandler {
		KartHandler kartHandler;

		public DriftingHandler() {
			kartHandler = LKernel.GetG<KartHandler>();
			kartHandler.OnTouchdown += OnTouchdown;
		}

		void OnTouchdown(Kart kart, CollisionWorld.ClosestRayResultCallback callback) {
			// if we're bouncing
			if (kart.IsBouncing) {

				kart.IsBouncing = false;
				// and want to drift
				if (kart.WantDriftState != DriftState.Normal) {
					// then we need to drift, if we aren't already!
					if (!kart.IsDrifting) {
						kart.StartDrifting();
					}
				}
				// otherwise if we aren't turning, but still drifting from before
				else if (kart.IsDrifting) {
					// then we keep drifting

				}
				// otherwise if we aren't drifting and aren't wanting to drift
				else {
					// do nothing
				}

			}
		}

		public void Detach() {
			kartHandler.OnTouchdown -= OnTouchdown;
		}
	}
}
