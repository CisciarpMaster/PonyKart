/*using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	public class KartRotater {
		Kart kart;
		float duration;
		float progress = 0;


		public KartRotater(Kart kartToRotate, float duration, Radian destAngle) {
			this.kart = kartToRotate;
			this.duration = duration;

			LKernel.GetG<PhysicsMain>().PreSimulate += PreSimulate;
		}

		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			// if the kart's gone, we can get rid of this too
			if (kart == null || kart.Vehicle.IsDisposed) {
				Detach();
				return;
			}
			// don't rotate if we're paused
			else if (Pauser.IsPaused)
				return;

			Vector3 locZ = kart.Body.Orientation.ZAxis;

			//if (kart.Body.Orientation.)
		}

		public void Detach() {
			LKernel.GetG<PhysicsMain>().PreSimulate -= PreSimulate;
		}
	}
}
*/