using BulletSharp;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	/// <summary>
	/// When a kart lands from a jump, this temporarily disables its wheels' friction
	/// </summary>
	public class Skidder : System.IDisposable {
		public Kart Kart;
		float Duration;
		float Progress = 0;


		public Skidder(Kart kart, float duration = 0.5f) {
			Kart = kart;
			Duration = duration;

			LKernel.GetG<PhysicsMain>().PreSimulate += Update;
		}

		/// <summary>
		/// It's just a linear function
		/// </summary>
		void Update(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (IsDisposed || Pauser.IsPaused)
				return;

			//System.Console.WriteLine(Kart.Body.AngularVelocity);

			Progress += evt.timeSinceLastFrame;
			if (Progress > Duration) {
				Kart.Vehicle.GetWheelInfo((int) WheelID.FrontLeft).FrictionSlip = Kart.WheelFL.FrictionSlip;
				Kart.Vehicle.GetWheelInfo((int) WheelID.FrontRight).FrictionSlip = Kart.WheelFR.FrictionSlip;
				Kart.Vehicle.GetWheelInfo((int) WheelID.BackLeft).FrictionSlip = Kart.WheelBL.FrictionSlip;
				Kart.Vehicle.GetWheelInfo((int) WheelID.BackRight).FrictionSlip = Kart.WheelBR.FrictionSlip;

				Dispose();
				return;
			}

			float fraction = Progress / Duration;

			Kart.Vehicle.GetWheelInfo((int) WheelID.FrontLeft).FrictionSlip = Kart.WheelFL.FrictionSlip * fraction;
			Kart.Vehicle.GetWheelInfo((int) WheelID.FrontRight).FrictionSlip = Kart.WheelFR.FrictionSlip * fraction;
			Kart.Vehicle.GetWheelInfo((int) WheelID.BackLeft).FrictionSlip = Kart.WheelBL.FrictionSlip * fraction;
			Kart.Vehicle.GetWheelInfo((int) WheelID.BackRight).FrictionSlip = Kart.WheelBR.FrictionSlip * fraction;

			Vector3 vec = new Vector3(Kart.Body.AngularVelocity.x, Kart.Body.AngularVelocity.y, Kart.Body.AngularVelocity.z);
			if (Kart.Body.AngularVelocity.x > 1)
				vec.x = 1;
			else if (Kart.Body.AngularVelocity.x < -1)
				vec.x = -1;

			if (Kart.Body.AngularVelocity.y > 2)
				vec.y = 2;
			else if (Kart.Body.AngularVelocity.y < -2)
				vec.y = -2;

			if (Kart.Body.AngularVelocity.z > 1)
				vec.z = 1;
			else if (Kart.Body.AngularVelocity.z < -1)
				vec.z = -1;

			Kart.Body.AngularVelocity = vec;
		}

		public bool IsDisposed = false;
		public void Dispose() {
			LKernel.GetG<PhysicsMain>().PreSimulate -= Update;
			IsDisposed = true;

			LKernel.Get<StopKartsFromRollingOverHandler>().Skidders.Remove(Kart);
		}
	}
}
