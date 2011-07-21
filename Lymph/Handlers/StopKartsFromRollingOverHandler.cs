using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Phys;
using Ponykart.Players;

namespace Ponykart.Handlers {
	/// <summary>
	/// This'll be a class that stops karts from rolling over and spinning around when they're in the air,
	/// but it isn't really working right now.
	/// At the moment it raycasts downwards and if the distance to the nearest thing is over than something, then we stop it from spinning in the air
	/// </summary>
	public class StopKartsFromRollingOverHandler : IDisposable {

		public StopKartsFromRollingOverHandler() {
			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			// TODO: raycast only every 0.1 or 0.3 or something seconds, but do the smoothing every frame
			//if (elapsed > 0/*.3f*/) {
			//	elapsed = 0;

				foreach (Player p in LKernel.Get<PlayerManager>().Players) {
					Kart kart = p.Kart;
					// don't bother raycasting for karts that aren't moving, or if we're paused
					if (kart == null || kart.Actor.IsSleeping || Pauser.IsPaused)
						continue;

					// get the kart's local X axis, so if you're on a steep slope it doesn't start screwing up
					Vector3 localYAxis = -kart.Node.GetLocalYAxis();
					Ray ray = new Ray(kart.Node.Position, localYAxis);
					RaycastHit hit;
					// TODO: check that the hit shape is either static or kinematic
					Shape closestShape = LKernel.Get<PhysXMain>().Scene.RaycastClosestShape(ray, ShapesTypes.All, out hit);
					
					// if the ray collided with something
					if (closestShape != null) {
						//Vector3 impact = hit.WorldImpact;
						float dist = hit.Distance;

						if (dist > 2f) {
							// stop it spinning
							kart.Actor.AngularVelocity = Vector3.ZERO;

							// oh god this took forever to figure out, fucking quaternions
							// so first we get the kart's orientation
							Matrix3 matrix = kart.Actor.GlobalOrientation;
							// then we basically get its local Y axis and average it with the global Y axis to make more of a smooth transition
							Vector3 avgY = (matrix.GetLocalYAxis() + Vector3.UNIT_Y) / 2f;
							// then set the matrix's Y axis to the averaged axis
							matrix.SetColumn(1, avgY);
							// and then update the actor with the new matrix
							kart.Actor.GlobalOrientation = matrix;
							
							// we can kinda combine this all into one line with our handy dandy extension methods
							//kart.LocalYAxis = (kart.LocalYAxis + Vector3.UNIT_Y) / 2f;
						}
					}
				}
			//}
			//elapsed += evt.timeSinceLastFrame;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}
