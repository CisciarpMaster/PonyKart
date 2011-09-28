using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// When a kart lands from a jump, this temporarily disables its wheels' friction
	/// </summary>
	public class Skidder {
		public Kart kart;
		float duration;
		float progress = 0;


		public Skidder(Kart kart, float duration = 0.5f) {
			this.kart = kart;
			this.duration = duration;

			LKernel.GetG<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// It's just a linear function
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (kart == null || Pauser.IsPaused)
				return;

			progress += evt.timeSinceLastFrame;
			if (progress > duration) {
				// get rid of the skidder
				Detach();
				return;
			}

			float fraction = progress / duration;
			// update friction
			kart.ForEachWheel(w => {
				w.Friction = w.FrictionSlip * fraction;
			});

			// limit angular velocity
			Vector3 vec = new Vector3(kart.Body.AngularVelocity.x, kart.Body.AngularVelocity.y, kart.Body.AngularVelocity.z);
			if (kart.Body.AngularVelocity.x > 1)
				vec.x = 1;
			else if (kart.Body.AngularVelocity.x < -1)
				vec.x = -1;

			if (kart.Body.AngularVelocity.y > 2)
				vec.y = 2;
			else if (kart.Body.AngularVelocity.y < -2)
				vec.y = -2;

			if (kart.Body.AngularVelocity.z > 1)
				vec.z = 1;
			else if (kart.Body.AngularVelocity.z < -1)
				vec.z = -1;

			kart.Body.AngularVelocity = vec;
		}

		public void Detach() {
			if (kart != null) {
				// reset it back to normal
				kart.ForEachWheel(w => {
					w.Friction = w.FrictionSlip;
				});

				LKernel.GetG<PhysicsMain>().PreSimulate -= Update;
				Skidder temp;
				LKernel.Get<KartHandler>().Skidders.TryRemove(kart, out temp);
				kart = null;
			}
		}
	}
}
