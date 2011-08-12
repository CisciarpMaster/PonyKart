using System;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;

namespace Ponykart.Handlers {
	/// <summary>
	/// Instead of doing 8 raycasts every frame to self-right stuff, instead we only raycast every few frames and then only run
	/// this when we need to self-right, and then get rid of it afterwards
	/// </summary>
	public class SelfRightingHandler : IDisposable {
		Kart kart;

		public SelfRightingHandler(Kart kartToFlip) {
			kart = kartToFlip;

			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		/// <summary>
		/// oh god this took forever to figure out. Fucking quaternions, how do they work
		/// 
		/// TODO: use constraints to stop the karts from flipping
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			// if the kart's gone, then we can get rid of this handler too
			if (kart == null || kart.Vehicle.IsDisposed) {
				Dispose();
				return true;
			}
			// don't self-right if we're paused
			else if (Pauser.IsPaused)
				return true;


			// so first we get the kart's orientation
			Matrix3 matrix = kart.Body.Orientation.ToRotationMatrix();
			// then we basically get its local Y axis and average it with the global Y axis to make more of a smooth transition
			Vector3 avgY = matrix.GetLocalYAxis();
			Vector3 locY = matrix.GetLocalYAxis();

			// first of all, if we're self righted enough, we can get rid of this handler
			if (locY.DirectionEquals(Vector3.UNIT_Y, new Degree(5))) {
				Dispose();
				return true;
			}

			// stop it spinning
			kart.Body.AngularVelocity = Vector3.ZERO;

			// are we upside down?
			if (locY.DirectionEquals(Vector3.NEGATIVE_UNIT_Y, new Degree(90))) {
				// if we are upside down, doing too many midpoints makes stuff go screwy
				Vector3 locX = matrix.GetLocalXAxis();
				matrix.SetColumn(0, Vector3.UNIT_X.MidPoint(locX).MidPoint(locX).MidPoint(locX));
				avgY = Vector3.UNIT_Y.MidPoint(locY);
			}
			else {
				// more midpoints means more smoothing
				avgY = Vector3.UNIT_Y.MidPoint(locY).MidPoint(locY).MidPoint(locY);
			}
			// then set the matrix's Y axis to the averaged axis
			matrix.SetColumn(1, avgY);
			// and then update the actor with the new matrix
			//kart.Body.Orientation = new Quaternion(matrix);

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			LKernel.Get<StopKartsFromRollingOverHandler>().SRHs.Remove(kart);
			kart = null;
		}
	}
}
