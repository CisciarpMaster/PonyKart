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
		public Kart _kart;
		float _duration;
		float _progress = 0;


		public Skidder(Kart kart, float duration = 0.5f) {
			_kart = kart;
			_duration = duration;

			LKernel.GetG<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// It's just a linear function
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (_kart == null || Pauser.IsPaused)
				return;

			_progress += evt.timeSinceLastFrame;
			if (_progress > _duration) {
				// reset friction back to normal and get rid of the skidder
				_kart.Vehicle.GetWheelInfo((int) WheelID.FrontLeft).FrictionSlip = _kart.WheelFL.FrictionSlip;
				_kart.Vehicle.GetWheelInfo((int) WheelID.FrontRight).FrictionSlip = _kart.WheelFR.FrictionSlip;
				_kart.Vehicle.GetWheelInfo((int) WheelID.BackLeft).FrictionSlip = _kart.WheelBL.FrictionSlip;
				_kart.Vehicle.GetWheelInfo((int) WheelID.BackRight).FrictionSlip = _kart.WheelBR.FrictionSlip;

				Detach();
				return;
			}

			float fraction = _progress / _duration;
			// update friction
			_kart.Vehicle.GetWheelInfo((int) WheelID.FrontLeft).FrictionSlip = _kart.WheelFL.FrictionSlip * fraction;
			_kart.Vehicle.GetWheelInfo((int) WheelID.FrontRight).FrictionSlip = _kart.WheelFR.FrictionSlip * fraction;
			_kart.Vehicle.GetWheelInfo((int) WheelID.BackLeft).FrictionSlip = _kart.WheelBL.FrictionSlip * fraction;
			_kart.Vehicle.GetWheelInfo((int) WheelID.BackRight).FrictionSlip = _kart.WheelBR.FrictionSlip * fraction;

			// limit angular velocity
			Vector3 vec = new Vector3(_kart.Body.AngularVelocity.x, _kart.Body.AngularVelocity.y, _kart.Body.AngularVelocity.z);
			if (_kart.Body.AngularVelocity.x > 1)
				vec.x = 1;
			else if (_kart.Body.AngularVelocity.x < -1)
				vec.x = -1;

			if (_kart.Body.AngularVelocity.y > 2)
				vec.y = 2;
			else if (_kart.Body.AngularVelocity.y < -2)
				vec.y = -2;

			if (_kart.Body.AngularVelocity.z > 1)
				vec.z = 1;
			else if (_kart.Body.AngularVelocity.z < -1)
				vec.z = -1;

			_kart.Body.AngularVelocity = vec;
		}

		public void Detach() {
			if (_kart != null) {
				LKernel.GetG<PhysicsMain>().PreSimulate -= Update;
				LKernel.Get<StopKartsFromRollingOverHandler>().Skidders.Remove(_kart);
				_kart = null;
			}
		}
	}
}
