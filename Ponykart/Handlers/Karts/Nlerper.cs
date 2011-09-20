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
		Quaternion _orientSrc;
		Quaternion _orientDest;
		float _progress = 0;
		float _duration;
		Kart _kart;

		public Nlerper(Kart kart, float duration, Quaternion orientDest) {
			_duration = duration;
			_orientSrc = kart.Body.Orientation;
			_orientDest = orientDest;
			_kart = kart;

			LKernel.GetG<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// nlerp!
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (_kart == null || Pauser.IsPaused)
				return;

			// don't do this more than we have to
			_progress += evt.timeSinceLastFrame;
			if (_progress > _duration) {
				Detach();
				return;
			}

			Quaternion delta = Quaternion.Nlerp(_progress / _duration, _orientSrc, _orientDest, true);
			_kart.Body.SetOrientation(delta);
		}

		public void Detach() {
			if (_kart != null) {
				LKernel.GetG<PhysicsMain>().PreSimulate -= Update;
				Nlerper temp;
				LKernel.Get<StopKartsFromRollingOverHandler>().Nlerpers.TryRemove(_kart, out temp);
				_kart = null;
			}
		}
	}
}
