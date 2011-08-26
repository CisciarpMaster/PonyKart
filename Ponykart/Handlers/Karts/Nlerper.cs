using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// A little class to help us nlerp things
	/// </summary>
	public class Nlerper : System.IDisposable {
		Quaternion OrientSrc;
		Quaternion OrientDest;
		float Progress = 0;
		float Duration;
		public Kart Kart;

		public Nlerper(Kart kart, float duration, Quaternion orientDest) {
			Duration = duration;
			OrientSrc = kart.Body.Orientation;
			OrientDest = orientDest;
			Kart = kart;

			LKernel.Get<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// nlerp!
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (IsDisposed || Pauser.IsPaused)
				return;

			// don't do this more than we have to
			Progress += evt.timeSinceLastFrame;
			if (Progress > Duration) {
				Dispose();
				return;
			}

			Quaternion delta = Quaternion.Nlerp(Progress / Duration, OrientSrc, OrientDest, true);
			Kart.Body.SetOrientation(delta);
		}

		public bool IsDisposed = false;
		public void Dispose() {
			LKernel.Get<PhysicsMain>().PreSimulate -= Update;
			IsDisposed = true;

			LKernel.Get<StopKartsFromRollingOverHandler>().Nlerpers.Remove(Kart);
		}
	}
}
