using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// A little class to help us nlerp things
	/// </summary>
	public class Nlerper {
		Quaternion orientSrc;
		Quaternion orientDest;
		float progress = 0;
		float duration;
		Kart kart;

		public Nlerper(Kart kart, float duration, Quaternion orientDest) {
			this.duration = duration;
			this.orientSrc = kart.Body.Orientation;
			this.orientDest = orientDest;
			this.kart = kart;

			LKernel.GetG<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// nlerp!
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (kart == null || Pauser.IsPaused)
				return;

			// don't do this more than we have to
			progress += evt.timeSinceLastFrame;
			if (progress > duration) {
				Detach();
				return;
			}

			Quaternion delta = Quaternion.Nlerp(progress / duration, orientSrc, orientDest, true);
			kart.Body.SetOrientation(delta);
		}

		public void Detach() {
			if (kart != null) {
				LKernel.GetG<PhysicsMain>().PreSimulate -= Update;
				Nlerper temp;
				LKernel.Get<KartHandler>().Nlerpers.TryRemove(kart, out temp);
				kart = null;
			}
		}
	}
}
