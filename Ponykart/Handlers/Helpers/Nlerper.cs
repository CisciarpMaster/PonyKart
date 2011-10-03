using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	public delegate void NlerpEvent(Nlerper nlerper, Kart kart);

	/// <summary>
	/// A little class to help us nlerp things
	/// </summary>
	public class Nlerper {
		Quaternion orientSrc;
		Quaternion orientDest;
		float progress = 0;
		float duration;
		Kart kart;

		/// <summary>
		/// This runs when the nlerper is finished doing its job. Use it for doing something that you need to do after it's done
		/// </summary>
		public static event NlerpEvent Finished;

		/// <summary>
		/// A nlerper is something that rotates a kart over a certain time to a certain orientation.
		/// Multiple nlerpers running at the same can cause problems, so you should check for other
		/// nlerpers and do something with them first before creating more.
		/// </summary>
		/// <param name="kart">The kart we want to rotate</param>
		/// <param name="duration">The time the rotation should take, in seconds</param>
		/// <param name="orientDest">The destination orientation the kart should be when this is done</param>
		public Nlerper(Kart kart, float duration, Quaternion orientDest) {
			this.duration = duration;
			this.orientSrc = kart.Body.Orientation;
			this.orientDest = orientDest;
			this.kart = kart;

			PhysicsMain.PreSimulate += PreSimulate;
		}

		/// <summary>
		/// nlerp!
		/// </summary>
		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (kart == null || Pauser.IsPaused)
				return;

			progress += evt.timeSinceLastFrame;
			if (progress > duration) {
				Detach();
				return;
			}

			Quaternion delta = Quaternion.Nlerp(progress / duration, orientSrc, orientDest, true);
			kart.Body.SetOrientation(delta);
		}

		/// <summary>
		/// Clean up
		/// </summary>
		public void Detach() {
			if (Finished != null)
				Finished(this, kart);

			if (kart != null) {
				PhysicsMain.PreSimulate -= PreSimulate;
				Nlerper temp;
				LKernel.GetG<KartHandler>().Nlerpers.TryRemove(kart, out temp);
				kart = null;
			}
		}
	}
}