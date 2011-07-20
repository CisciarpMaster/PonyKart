using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Actors;
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
			if (elapsed > 0.3f) {
				elapsed = 0;

				foreach (Player p in LKernel.Get<PlayerManager>().Players) {
					Kart kart = p.Kart;
					// don't bother raycasting for karts that aren't moving
					if (kart == null || kart.Actor.IsSleeping)
						continue;

					Ray ray = new Ray(kart.Actor.GlobalPosition, Vector3.NEGATIVE_UNIT_Y);
					RaycastHit hit;
					//float dist;
					Shape closestShape = LKernel.Get<PhysXMain>().Scene.RaycastClosestShape(ray, ShapesTypes.All, out hit);
					// TODO: check that the hit shape is either static or kinematic
					
					// if the ray collided with something
					if (closestShape != null) {
						//Vector3 impact = hit.WorldImpact;
						float dist = hit.Distance;
						
						//Console.WriteLine(kart.Name + kart.ID + ": " + impact + "   " + dist);

						if (dist > 2f) {
							// I have no idea what I'm doing here

							/*Radian yangle = new Radian();
							Vector3 yaxis = kart.Actor.GlobalOrientationQuaternion.YAxis;
							//Vector3 yaxis = Vector3.UNIT_Y;
							kart.Actor.GlobalOrientationQuaternion.ToAngleAxis(out yangle, out yaxis);

							Console.WriteLine(kart.Name + ": " + yangle.ValueDegrees + "   " + yaxis);

							kart.Actor.GlobalOrientationQuaternion = new Vector3(0, yangle.ValueRadians, 0).ToQuaternion();*/

							//Radian x, y, z;
							//kart.Actor.GlobalOrientation.ToEulerAnglesXYZ(out x, out y, out z);
							//Console.WriteLine(kart.Name + kart.ID + ": " + x.ValueDegrees + " " + y.ValueDegrees + " " + z.ValueDegrees);

							/*bool hasXChanged = true, hasZChanged = true;
							if (x.ValueDegrees < 140 && x.ValueDegrees >= 90)
								x = 140;
							else if (x.ValueDegrees > 40 && x.ValueDegrees <= 90)
								x = 40;
							else if (x.ValueDegrees > -140 && x.ValueDegrees <= -90)
								x = -140;
							else if (x.ValueDegrees < -40 && x.ValueDegrees >= -90)
								x = -40;
							else
								hasXChanged = false;

							if (z.ValueDegrees < 140 && z.ValueDegrees >= 90)
								z = 140;
							else if (z.ValueDegrees > 40 && z.ValueDegrees <= 90)
								z = 40;
							else if (z.ValueDegrees > -140 && z.ValueDegrees <= -90)
								z = -140;
							else if (z.ValueDegrees < -40 && z.ValueDegrees >= -90)
								z = -40;
							else
								hasZChanged = false;


							if (hasXChanged || hasZChanged) {
								Matrix3 mat = new Matrix3();
								mat.FromEulerAnglesXYZ(x, y, z);
								kart.Actor.GlobalOrientation = mat;
							}*/

							/*kart.Node.Orientation = Quaternion.IDENTITY;
							kart.Node.Yaw(y);
							//kart.Node.Yaw(yangle);
							kart.Actor.GlobalOrientationQuaternion = kart.Node.Orientation;*/

							kart.Actor.AngularVelocity = Vector3.ZERO;
							
						}
					}
				}
			}
			elapsed += evt.timeSinceLastFrame;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}
